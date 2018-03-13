{ File   : VDXExt.dpr
  Author : Deniz Oezmen
  Created: 2005-05-27, based on VDXEXT.PAS created ~2000
  Changed: 2005-11-29

  The 7th Guest VDX extractor.
}
program VDXExt;

{$APPTYPE CONSOLE}

uses
  SysUtils, StrUtils, SaveBMP2, AviWriter, Graphics;

type
  TVDXHeader   = packed record
                   ID      : Word;
                   Unknown1,
                   Unknown2,
                   Unknown3,
                   Unknown4,
                   Unknown5,            // max. length mask?
                   Unknown6: Byte
                 end;

  TBlockHeader = packed record
                   BlockType,
                   PlayCmd   : Byte;
                   BlockSize : Longword;
                   LengthMask,
                   BitsForLen: Byte     // bits of 16 to use for length
                 end;

  TBMPHeader   = packed record
                   Width,
                   Height,
                   PalBits: Word
                 end;

  TBuffer      = array of Byte;

const
  WAVHeader: packed array[0..43] of Char =
    ( 'R',  'I',  'F',  'F', #$00, #$00, #$00, #$00,  'W',  'A',  'V',  'E',
      'f',  'm',  't',  ' ', #$10, #$00, #$00, #$00, #$01, #$00, #$01, #$00,
     #$22, #$56, #$00, #$00, #$22, #$56, #$00, #$00, #$01, #$00, #$08, #$00,
      'd',  'a',  't',  'a', #$00, #$00, #$00, #$00);

var
  OutWAV,
  VDXFile  : file;
  VDXHeader: TVDXHeader;
  Palette3 : TPalette3;
  BMPHeader: TBMPHeader;
  Sync,
  MakeAVI  : Boolean;
  FrameBuf : TBuffer;
  i,
  VidFrames,
  VidAdded,
  AudFrames,
  AudAdded,
  AudSize  : Longword;
  Bitmap   : TBitmap;
  AviWr    : TAviWriter;

function IntToStrL(i, l: Integer): string;
begin
  Result := IntToStr(i);

  while Length(Result) < l do
    Result := '0' + Result
end;

function Prefix(FileName: string): string;
var
  i: Longint;
begin
  FileName := ExpandFileName(FileName);

  i := Length(FileName) + 1;
  repeat
    Dec(i)
  until (i = 0) or (PosEx('.', FileName, i) > 0);

  if (i = 0) or (PosEx('\', FileName, i + 1) > 0) then
    Result := FileName
  else
    Result := Copy(FileName, 1, i - 1)
end;

function OpenVDXFile(var VDXFile: file; VDXName: string): Boolean;
begin
  FileMode := fmShareDenyWrite;
  AssignFile(VDXFile, VDXName);
  
  {$I-}
  Reset(VDXFile, 1);
  {$I+}
  OpenVDXFile := (IOResult = 0)
end;

function ReadHeader: Boolean;
var
  BytesRead: Longword;
begin
  BlockRead(VDXFile, VDXHeader, SizeOf(TVDXHeader), BytesRead);
  ReadHeader := (SizeOf(TVDXHeader) = BytesRead) and (VDXHeader.ID = 37479)
end;

procedure DecompressBlock(InBuf: TBuffer; BlockHeader: TBlockHeader;
  var OutBuf: TBuffer);
var
  N,
  F,
  Offset,
  Length,
  OfsLen   : Word;
  Threshold,
  Flags,
  b        : Byte;
  i, j,
  InBufPos,
  OutBufPos,
  HisBufPos: Longint;
  HisBuf   : TBuffer;
begin
  // initialize LZSS parameters for this block
  N := 1 shl ($f - BlockHeader.BitsForLen + 1);
  F := 1 shl (BlockHeader.BitsForLen);
  Threshold := 3;
  HisBufPos := N - F;

  SetLength(HisBuf, N);
  FillChar(HisBuf[0], N, 0);

  SetLength(OutBuf, 0);
  OutBufPos := 0;

  InBufPos := 0;

  // start extracting block contents
  while Longword(InBufPos) < BlockHeader.BlockSize - 1 do
  begin
    // read bit field 
    Flags := InBuf[InBufPos];
    Inc(InBufPos);

    // process bit field 
    for i := 1 to 8 do
    begin
      if Longword(InBufPos) < BlockHeader.BlockSize - 1 then
      begin
        // check for buffer window reference 
        if (Flags and 1 = 0) then
        begin
          // read offset and length 
          OfsLen := InBuf[InBufPos] + (InBuf[InBufPos + 1] shl 8);
          Inc(InBufPos, 2);

          // check for end marker 
          if OfsLen = 0 then
            Break;

          // derive offset and length values 
          Length := (OfsLen and BlockHeader.LengthMask) + Threshold;
          Offset := (HisBufPos - (OfsLen shr BlockHeader.BitsForLen))
            and (N - 1);

          // peek into buffer 
          SetLength(OutBuf, High(OutBuf) + Length + 1);
          for j := 0 to Length - 1 do
          begin
            b := HisBuf[(Offset + j) and (N - 1)];
            OutBuf[OutBufPos] := b;
            Inc(OutBufPos);
            HisBuf[HisBufPos] := b;
            HisBufPos := (HisBufPos + 1) and (N - 1)
          end
        end
        else
        // copy literally 
        begin
          SetLength(OutBuf, High(OutBuf) + 2);
          b := InBuf[InBufPos];
          Inc(InBufPos);
          OutBuf[OutBufPos] := b;
          HisBuf[HisBufPos] := b;
          Inc(OutBufPos);
          HisBufPos := (HisBufPos + 1) and (N - 1)
        end;

        Flags := Flags shr 1
      end
    end
  end
end;

procedure WriteBMP(VDXName: string; InBuf: TBuffer);
var
  i, j,
  c0, c1: Byte;
  x, y,
  Map   : Word;
  OutBuf: TBuffer;
begin
  Inc(VidFrames);

  // get BMP header 
  BMPHeader.Width := InBuf[0] + InBuf[1] shl 8;
  BMPHeader.Height := InBuf[2] + InBuf[3] shl 8;
  BMPHeader.PalBits := InBuf[4] + InBuf[5] shl 8;

  if BMPHeader.PalBits <> 8 then
  begin
    WriteLn('  only 8-bit paletted BMPs supported');
    Exit
  end;

  // cut header
  InBuf := Copy(InBuf, SizeOf(BMPHeader), High(InBuf) - SizeOf(BMPHeader) + 1);

  // get palette 
  for i := 0 to 255 do
  begin
    Palette3[i].R := InBuf[i * 3];
    Palette3[i].G := InBuf[i * 3 + 1];
    Palette3[i].B := InBuf[i * 3 + 2]
  end;

  // cut palette 
  InBuf := Copy(InBuf, SizeOf(Palette3), High(InBuf) - SizeOf(Palette3) + 1);

  BMPHeader.Width := BMPHeader.Width * 4;
  BMPHeader.Height := BMPHeader.Height * 4;

  SetLength(OutBuf, BMPHeader.Width * BMPHeader.Height);

  // decode image
  for y := 0 to (BMPHeader.Height div 4) - 1 do
  begin
    for x := 0 to (BMPHeader.Width div 4) - 1 do
    begin
      c1 := InBuf[y * BMPHeader.Width + x * 4];
      c0 := InBuf[y * BMPHeader.Width + x * 4 + 1];
      Map := InBuf[y * BMPHeader.Width + x * 4 + 2] +
        InBuf[y * BMPHeader.Width + x * 4 + 3] shl 8;

      for i := 0 to 3 do
      begin
        for j := 0 to 3 do
        begin
          if Map and $8000 = 0 then
            OutBuf[(y * 4 + i) * BMPHeader.Width + x * 4 + j] := c0
          else
            OutBuf[(y * 4 + i) * BMPHeader.Width + x * 4 + j] := c1;
                                   Map := Map shl 1
        end
      end
    end
  end;

  Dump8BitBMP(Prefix(VDXName) + '#' + IntToStrL(VidFrames - 1, 4) + '.bmp',
    BMPHeader.Width, BMPHeader.Height, Palette3, OutBuf);

  SetLength(FrameBuf, High(OutBuf) + 1);
  FrameBuf := Copy(OutBuf, 0, High(OutBuf) + 1)
end;

procedure WriteVidFrame(VDXName: string; InBuf: TBuffer);
const
  MapField: array[0..191] of Byte =
    ($00, $c8, $80, $ec, $c8, $fe, $ec, $ff, $fe, $ff, $00, $31, $10, $73, $31,
     $f7, $73, $ff, $f7, $ff, $80, $6c, $c8, $36, $6c, $13, $10, $63, $31, $c6,
     $63, $8c, $00, $f0, $00, $ff, $f0, $ff, $11, $11, $33, $33, $77, $77, $66,
     $66, $cc, $cc, $f0, $0f, $ff, $00, $cc, $ff, $76, $00, $33, $ff, $e6, $0e,
     $ff, $cc, $70, $67, $ff, $33, $e0, $6e, $00, $48, $80, $24, $48, $12, $24,
     $00, $12, $00, $00, $21, $10, $42, $21, $84, $42, $00, $84, $00, $88, $f8,
     $44, $00, $32, $00, $1f, $11, $e0, $22, $00, $4c, $8f, $88, $70, $44, $00,
     $23, $11, $f1, $22, $0e, $c4, $00, $3f, $f3, $cf, $fc, $99, $ff, $ff, $99,
     $44, $44, $22, $22, $ee, $cc, $33, $77, $f8, $00, $f1, $00, $bb, $00, $dd,
     $0c, $0f, $0f, $88, $0f, $f1, $13, $b3, $19, $80, $1f, $6f, $22, $ec, $27,
     $77, $30, $67, $32, $e4, $37, $e3, $38, $90, $3f, $cf, $44, $d9, $4c, $99,
     $4c, $55, $55, $3f, $60, $77, $60, $37, $62, $c9, $64, $cd, $64, $d9, $6c,
     $ef, $70, $00, $0f, $f0, $00, $00, $00, $44, $44, $22, $22);
var
  i, j, k : Longint;
  Map,
  LocPalSz,
  x, y    : Word;
  c0, c1  : Byte;
begin
  Inc(VidFrames);

  // check for local palette adaptations
  LocPalSz := Word(InBuf[0]) + Word(InBuf[1]) shl 8;
  k := 0;

  // alter palette according to bitfield
  if LocPalSz > 0 then
  begin
    for i := 0 to 15 do
    begin
      Map := Word(InBuf[i * 2 + 2]) + Word(InBuf[i * 2 + 3]) shl 8;
      for j := 0 to 15 do
      begin
        if Map and $8000 <> 0 then
        begin
          Palette3[i * 16 + j].R := InBuf[34 + k];
          Palette3[i * 16 + j].G := InBuf[34 + k + 1];
          Palette3[i * 16 + j].B := InBuf[34 + k + 2];
          Inc(k, 3)
        end;
        Map := Map shl 1
      end
    end;
  end;

  x := 0;
  y := 0;

  i := LocPalSz + 2;

  // decode image
  while i < High(InBuf) do
  begin
    // process opcodes
    case InBuf[i] of
      // use predefined map
        0.. 95: begin
                  Map := Word(MapField[Word(InBuf[i] shl 1)]) +
                         Word(MapField[Word(InBuf[i] shl 1) + 1]) shl 8;
                  c1 := InBuf[i + 1];
                  c0 := InBuf[i + 2];
                  for j := 0 to 15 do
                  begin
                    if Map and $8000 = 0 then
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := c0
                    else
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := c1;
                    Map := Map shl 1
                  end;
                  Inc(x, 4);
                  Inc(i, 2)
                end;
      // read 16 individual colours
       96     : begin
                  for j := 0 to 15 do
                    FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                      x + (j mod 4)] := InBuf[i + j + 1];
                  Inc(x, 4);
                  Inc(i, 16)
                end;
      // skip one line
       97     : begin
                  Inc(y, 4);
                  x := 0
                end;
      // skip blocks within a line
       98..107: Inc(x, Word((InBuf[i] - 98)) shl 2);
      // solid fill area
      108..117: begin
                  for k := 1 to InBuf[i] - 107 do
                  begin
                    for j := 0 to 15 do
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := InBuf[i + 1];
                    Inc(x, 4);
                  end;
                  Inc(i, 1)
                end;
      // multiple single solid fills
      118..127: begin
                  for k := 1 to InBuf[i] - 117 do
                  begin
                    for j := 0 to 15 do
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := InBuf[i + k];
                    Inc(x, 4);
                  end;
                  Inc(i, InBuf[i] - 117)
                end;
      // use local map
      128..255: begin
                  Map := Word(InBuf[i]) + Word(InBuf[i + 1]) shl 8;
                  c1 := InBuf[i + 2];
                  c0 := InBuf[i + 3];
                  for j := 0 to 15 do
                  begin
                    if Map and $8000 = 0 then
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := c0
                    else
                      FrameBuf[(y + (j div 4)) * BMPHeader.Width +
                        x + (j mod 4)] := c1;
                    Map := Map shl 1
                  end;
                  Inc(x, 4);
                  Inc(i, 3)
                end
    end;
    Inc(i)
  end;

  Dump8BitBMP(Prefix(VDXName) + '#' + IntToStrL(VidFrames - 1, 4) + '.bmp',
    BMPHeader.Width, BMPHeader.Height, Palette3, FrameBuf)
end;

procedure WriteAudFrame(Buffer: TBuffer);
begin
  BlockWrite(OutWAV, Buffer[0], High(Buffer) + 1);
  Inc(AudSize, High(Buffer) + 1)
end;

function OpenWAV(VDXName: string; var OutWAV: file): Boolean;
begin
  FileMode := fmOpenReadWrite;
  Assign(OutWAV, Prefix(VDXName) + '.wav');

  {$I-}
  Rewrite(OutWAV, 1);
  {$I+}

  if IOResult = 0 then
  begin
    OpenWAV := True;
    BlockWrite(OutWAV, WAVHeader[0], SizeOf(WAVHeader))
  end
  else
    OpenWAV := False
end;

procedure CloseWAV(var OutWAV: file);
var
  Size: Longword;
begin
  // finalize WAV header
  Size := FileSize(OutWAV) - SizeOf(WAVHeader) + 36;
  Seek(OutWAV, 4);
  BlockWrite(OutWAV, Size, 4);

  Dec(Size, 36);
  Seek(OutWAV, 40);
  BlockWrite(OutWAV, Size, 4);

  Close(OutWAV)
end;

procedure CreateAVI(VDXName: string);
var
  i: Longword;
  f: File;
begin
  Write('  creating AVI file, please wait ... ');

  // set AVI parameters
  AviWr := TAviWriter.Create(nil);
  AviWr.Width := BMPHeader.Width;
  AviWr.Height := BMPHeader.Height;
  AviWr.FrameTime := 66666667;  // assumes 15 fps
  AviWr.Stretch := False;
  AviWr.FileName := Prefix(VDXName) + '.avi';

  // add wave file if one exists
  if AudFrames > 0 then
    AviWr.WavFileName := Prefix(VDXName) + '.wav'
  else
    AviWr.WavFileName := '';

  // add frames
  for i := 0 to VidFrames - 1 do
  begin
    Bitmap := TBitmap.Create;
    Bitmap.LoadFromFile(Prefix(VDXName) + '#' + IntToStrL(i, 4) + '.bmp');
    AviWr.Bitmaps.Add(Bitmap)
  end;

  AviWr.Write;

  // clear frames and delete bitmaps
  for i := VidFrames - 1 downto 0 do
  begin
    FileMode := fmOpenReadWrite;
    Assign(f, Prefix(VDXName) + '#' + IntToStrL(i, 4) + '.bmp');
    Erase(f);
    TBitmap(AviWr.Bitmaps[i]).Free;
    AviWr.Bitmaps.Delete(i)
  end;

  // delete wave file
  if AviWr.WavFileName <> '' then
  begin
    FileMode := fmOpenReadWrite;
    Assign(f, AviWr.WavFileName);
    Erase(f)
  end;

  AviWr.Free;

  WriteLn('done')
end;

procedure AddVidFrame(VDXName: string);
begin
  Inc(VidFrames);
  Inc(VidAdded);

  Dump8BitBMP(Prefix(VDXName) + '#' + IntToStrL(VidFrames - 1, 4) + '.bmp',
    BMPHeader.Width, BMPHeader.Height, Palette3, FrameBuf)
end;

procedure PadVideo(VDXName: string);
begin
  while VidFrames < AudFrames do
    AddVidFrame(VDXName)
end;

procedure PadAudio(var OutWAV: file);
var
  Buffer: TBuffer;
  Fill  : Longword;
begin
  if AudSize mod 1470 > 0 then
  begin
    Fill := 1470 - (AudSize mod 1470);
    SetLength(Buffer, Fill);
    FillChar(Buffer[0], Fill, 127);
    BlockWrite(OutWAV, Buffer[0], Fill);
    Inc(AudFrames)
  end;

  SetLength(Buffer, 1470);
  FillChar(Buffer[0], 1470, 127);

  while AudFrames < VidFrames do
  begin
    BlockWrite(OutWAV, Buffer[0], 1470);
    Inc(AudFrames);
    Inc(AudAdded)
  end;

  Finalize(Buffer)
end;

procedure ExpandFile(VDXName: string);
var
  WAVOpen    : Boolean;
  InBuf,
  OutBuf     : TBuffer;
  BlockHeader: TBlockHeader;
begin
  WAVOpen := false;

  VidFrames := 0;
  VidAdded  := 0;
  AudFrames := 0;
  AudAdded  := 0;
  AudSize   := 0;

  // go through VDX blocks 
  while not Eof(VDXFile) do
  begin
    // read block header
    BlockRead(VDXFile, BlockHeader, SizeOf(TBlockHeader));

    // read block content 
    SetLength(InBuf, BlockHeader.BlockSize);
    BlockRead(VDXFile, InBuf[0], BlockHeader.BlockSize);

    // check block compression 
    if (BlockHeader.BitsForLen = 0) then
      OutBuf := Copy(InBuf)
    else
      DecompressBlock(InBuf, BlockHeader, OutBuf);

    // check for known media type 
    case BlockHeader.BlockType of
        0: case BlockHeader.PlayCmd of
             103: if Sync then
                    AddVidFrame(VDXName);
           else
             WriteLn('  unsupported command ', BlockHeader.PlayCmd,
               ' found in block of type 0x00')
           end;
       32: case BlockHeader.PlayCmd of
             103: WriteBMP(VDXName, OutBuf);
             119: WriteBMP(VDXName, OutBuf)
           else
             WriteLn('  unsupported command ', BlockHeader.PlayCmd,
               ' found in block of type 0x20')
           end;
       37: case BlockHeader.PlayCmd of
             103: WriteVidFrame(VDXName, OutBuf);
             119: WriteVidFrame(VDXName, OutBuf)
           else
             WriteLn('  unsupported command ', BlockHeader.PlayCmd,
               ' found in block of type 0x25')
           end;
      128: begin
             case BlockHeader.PlayCmd of
               103: begin
                      if not WAVOpen then
                        WAVOpen := OpenWAV(VDXName, OutWAV);
                      if WAVOpen then
                        WriteAudFrame(OutBuf)
                    end;
               119: begin
                      if not WAVOpen then
                        WAVOpen := OpenWAV(VDXName, OutWAV);
                      if WAVOpen then
                        WriteAudFrame(OutBuf)
                    end
             else
               WriteLn('  unsupported command ', BlockHeader.PlayCmd,
                 ' found in block of type 0x80')
             end
           end
    else
      WriteLn('  unsupported block type 0x', IntToHex(BlockHeader.BlockType, 2),
        ' found')
    end;

    Finalize(InBuf);
    Finalize(OutBuf)
  end;

  AudFrames := AudSize div 1470;

  if Sync then
  begin
    if not WAVOpen then
      WAVOpen := OpenWAV(VDXName, OutWAV);
    if WAVOpen then
      PadAudio(OutWAV);
    PadVideo(VDXName)
  end;

  if WAVOpen then
    CloseWAV(OutWAV);

  // print some statistics
  Write('  ', VidFrames, ' video frame(s) found');
  if VidAdded > 0 then
    WriteLn(', including ', VidAdded, ' added for synchronization')
  else
    WriteLn;
  Write('  ', AudFrames, ' audio frame(s) found');
  if AudAdded > 0 then
    WriteLn(', including ', AudAdded, ' added for synchronization')
  else
    WriteLn;

  if MakeAVI then
    CreateAVI(VDXName)
end;

procedure ParseFiles(FilePara: string);
var
  DirPos,
  i        : Byte;
  FileName,
  Dir,
  VDXName  : string;
  SearchRec: TSearchRec;
  PFilePara: PChar;
begin
  GetMem(PFilePara, Length(FilePara) + 1);
  StrPCopy(PFilePara, FilePara);

  // get the directory information from FilePara
  DirPos := 0;
  for i := 1 to Length(FilePara) do
    if FilePara[i] in [':', '\'] then
      DirPos := i;

  Dir := Copy(FilePara, 1, DirPos);

  // search for all files matching FilePara 
  if FindFirst(PFilePara, faAnyFile - faDirectory - faVolumeID,
    SearchRec) = 0 then
  begin
    repeat
      FileName := SearchRec.Name;
      VDXName := Dir + FileName;

      // if this file is accessible, analyze it
      if OpenVDXFile(VDXFile, VDXName) then
        if ReadHeader then
        begin
          Writeln('Expanding ', VDXName, ' ...');
          ExpandFile(VDXName);
          Close(VDXFile)
        end
        else
          Writeln(VDXName, ' is not a valid VDX file.')
      else
        Writeln(VDXName, ' not accessible')
    until FindNext(SearchRec) <> 0
  end
  else
    Writeln(FilePara, ' not found')
end;

begin
  if ParamCount >= 1 then
  begin
    MakeAVI := False;
    Sync    := False;
    for i := 2 to ParamCount do
    begin
      MakeAVI := MakeAVI or (UpperCase(ParamStr(i)) = '/AVI');
      Sync    := Sync    or (UpperCase(ParamStr(i)) = '/SYNC')
    end;
    ParseFiles(ParamStr(1))
  end
  else
    Writeln('Incorrect number of parameters.')
end.
