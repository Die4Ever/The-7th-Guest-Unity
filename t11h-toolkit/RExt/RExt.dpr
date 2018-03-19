{ File   : RExt.dpr
  Author : Deniz Oezmen
  Created: 2005-10-05, based on ROLANALY.PAS created 2002-02-19
  Changed: 2005-10-14

  The 11th Hour ROL/ROQ/RNR extractor.
}
program RExt;

{$APPTYPE CONSOLE}

uses
  SysUtils, StrUtils;

type
  TBlockHeader = packed record
                   BlockType,
                   ID       : Byte;
                   DataSize : Longword;
                   Info     : Word;
                 end;

  TBuffer      = array of Byte;

const
  WAVHeader: packed array[0..43] of Char =
    ( 'R',  'I',  'F',  'F', #$00, #$00, #$00, #$00,  'W',  'A',  'V',  'E',
      'f',  'm',  't',  ' ', #$10, #$00, #$00, #$00, #$01, #$00, #$02, #$00,
     #$22, #$56, #$00, #$00, #$88, #$58, #$01, #$00, #$04, #$00, #$10, #$00,
      'd',  'a',  't',  'a', #$00, #$00, #$00, #$00);

var
  RFile    : file;
  WAVOpen  : Boolean;
  VidFrames,
  AudFrames: Longword;

function IntToStrL(i, l: Integer): string;
begin
  Result := IntToStr(i);

  while Length(Result) < l do
    Result := '0' + Result
end;

function OpenRFile(var RFile: file; RName: string): Boolean;
begin
  FileMode := fmShareDenyWrite;
  AssignFile(RFile, RName);
  
  {$I-}
  Reset(RFile, 1);
  {$I+}
  OpenRFile := (IOResult = 0)
end;

function ReadHeader: Boolean;
var
  BytesRead  : Longword;
  BlockHeader: TBlockHeader;
begin
  BlockRead(RFile, BlockHeader, SizeOf(TBlockHeader), BytesRead);
  ReadHeader := (SizeOf(TBlockHeader) = BytesRead) and
    (BlockHeader.BlockType = 132) and (BlockHeader.ID = 16)
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

function OpenJPG(RName: string; var OutJPG: file): Boolean;
begin
  FileMode := fmOpenReadWrite;
  Assign(OutJPG, Prefix(RName) + '#' + IntToStrL(VidFrames, 4) + '.jpg');
  
  {$I-}
  Rewrite(OutJPG, 1);
  {$I+}
  OpenJPG := (IOResult = 0)
end;

procedure CloseJPG(var OutJPG: file);
begin
  Close(OutJPG)
end;

function OpenWAV(ROLName: string; var OutWAV: file): Boolean;
begin
  FileMode := fmOpenReadWrite;
  Assign(OutWAV, Copy(ROLName, 1, Pos('.', ROLName) - 1) + '.wav');

  {$I-}
  Rewrite(OutWAV, 1);
  {$I+}
  Result := (IOResult = 0);

  if Result then
  begin
    {$I+}
    BlockWrite(OutWAV, WAVHeader[0], SizeOf(WAVHeader));
    {$I-}
    Result := (IOResult = 0)
  end
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

  Close(OutWAV);

  WAVOpen := False
end;

procedure WriteMonoAudioFrame(RName: string; var OutWAV: file;
  BlockHeader: TBlockHeader);
var
  Buffer    : TBuffer;
  OutBuf    : array of Word;
  i         : Longword;
  InSample,
  OutSample0: Word;
begin
  // read block content 
  SetLength(Buffer, BlockHeader.DataSize);
  BlockRead(RFile, Buffer[0], BlockHeader.DataSize);

  // open wave file 
  if not WAVOpen then
    WAVOpen := OpenWAV(RName, OutWAV);

  if not WAVOpen then
  begin
    WriteLn('  Error writing WAV file');
    Exit
  end;

  // initialize output buffer 
  SetLength(OutBuf, BlockHeader.DataSize shl 1);

  // decode data 
  OutSample0 := BlockHeader.Info xor $8000;
  for i := 0 to BlockHeader.DataSize - 1 do
  begin
    InSample := Buffer[i];
    if (InSample and $80) <> 0 then
    begin
      InSample := InSample and $7f;
      InSample := InSample * InSample;
      OutSample0 := OutSample0 - InSample
    end
    else
    begin
      InSample := InSample * InSample;
      OutSample0 := OutSample0 + InSample
    end;
    OutBuf[i shl 1] := OutSample0;
    OutBuf[i shl 1 + 1] := OutSample0
  end;

  // write block content 
  BlockWrite(OutWAV, OutBuf[0], BlockHeader.DataSize shl 2);

  // clean up 
  Finalize(Buffer);
  Finalize(OutBuf);

  Inc(AudFrames)
end;

procedure WriteStereoAudioFrame(RName: string; var OutWAV: file;
  BlockHeader: TBlockHeader);
var
  Buffer    : TBuffer;
  OutBuf    : array of Word;
  i         : Longword;
  InSample,
  OutSample0,
  OutSample1: Word;
begin
  // read block content 
  SetLength(Buffer, BlockHeader.DataSize);
  BlockRead(RFile, Buffer[0], BlockHeader.DataSize);

  // open wave file 
  if not WAVOpen then
    WAVOpen := OpenWAV(RName, OutWAV);

  if not WAVOpen then
  begin
    WriteLn('  Error writing WAV file');
    Exit
  end;

  // initialize output buffer 
  SetLength(OutBuf, BlockHeader.DataSize shl 1);

  // decode data 
  OutSample0 := (BlockHeader.Info and $ff00) xor $8000;
  OutSample1 := ((BlockHeader.Info and $00ff) shl 8) xor $8000;
  for i := 0 to (BlockHeader.DataSize shr 1) - 1 do
  begin
    InSample := Buffer[i shl 1];
    if (InSample and $80) <> 0 then
    begin
      InSample := InSample and $7f;
      InSample := InSample * InSample;
      OutSample0 := OutSample0 - InSample
    end
    else
    begin
      InSample := InSample * InSample;
      OutSample0 := OutSample0 + InSample
    end;
    OutBuf[i shl 1] := OutSample0;

    InSample := Buffer[i shl 1 + 1];
    if (InSample and $80) <> 0 then
    begin
      InSample := InSample and $7f;
      InSample := InSample * InSample;
      OutSample1 := OutSample1 - InSample
    end
    else
    begin
      InSample := InSample * InSample;
      OutSample1 := OutSample1 + InSample
    end;
    OutBuf[i shl 1 + 1] := OutSample1
  end;

  // write block content 
  BlockWrite(OutWAV, OutBuf[0], BlockHeader.DataSize shl 1);

  // clean up 
  Finalize(Buffer);
  Finalize(OutBuf);

  Inc(AudFrames)
end;

procedure WriteStillFrame(RName: string; BlockHeader: TBlockHeader);
var
  OutJPG: file;
  Buffer: TBuffer;
begin
  SetLength(Buffer, BlockHeader.DataSize);
  BlockRead(RFile, Buffer[0], BlockHeader.DataSize);

  if not OpenJPG(RName, OutJPG) then
  begin
    WriteLn('  Error writing JPG file');
    Exit
  end;

  BlockWrite(OutJPG, Buffer[0], BlockHeader.DataSize);

  CloseJPG(OutJPG);

  Inc(VidFrames)
end;

procedure ExpandFile(RName: string);
var
  BlockHeader: TBlockHeader;
  OutWAV     : file;
begin
  VidFrames := 0;
  AudFrames := 0;

  // go through R blocks 
  while not Eof(RFile) do
  begin
    // read block header 
    BlockRead(RFile, BlockHeader, SizeOf(TBlockHeader));

    if BlockHeader.ID <> 16 then
      WriteLn('Warning: Unexpected ID value of ', BlockHeader.ID);

    // determine block type 
    case BlockHeader.BlockType of
      // picture block
       18: WriteStillFrame(RName, BlockHeader);
      // mono audio block
       32: WriteMonoAudioFrame(RName, OutWAV, BlockHeader);
      // stereo audio block
       33: WriteStereoAudioFrame(RName, OutWAV, BlockHeader);
      // audio container block (?)
       48: ;
      // start block
      132: begin
             WriteLn('  Error: Start block in unexpected location, aborting');
             Break
           end
    else
      begin
        WriteLn('  Warning: Unsupported block type 0x',
          IntToHex(BlockHeader.BlockType, 2), ', skipping ',
          BlockHeader.DataSize, ' bytes');
        Seek(RFile, FilePos(RFile) + Integer(BlockHeader.DataSize))
      end
    end
  end;

  if WAVOpen then
    CloseWAV(OutWAV);

  // some statistics 
  if VidFrames > 0 then
    WriteLn('  ', VidFrames, ' video frame(s) found');
  if AudFrames > 0 then
    WriteLn('  ', AudFrames, ' audio frame(s) found')
end;

procedure ParseFiles(FilePara: string);
var
  DirPos,
  i        : Byte;
  FileName,
  Dir,
  RName  : string;
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
      RName := Dir + FileName;

      // if this file is accessible, analyze it
      if OpenRFile(RFile, RName) then
        if ReadHeader then
        begin
          Writeln('Expanding ', RName, ' ...');
          ExpandFile(RName);
          Close(RFile)
        end
        else
          Writeln(RName, ' is not a valid ROL/ROQ/RNR file')
      else
        Writeln(RName, ' not accessible')
    until FindNext(SearchRec) <> 0
  end
  else
    Writeln(FilePara, ' not found')
end;

begin
  if ParamCount = 1 then
    ParseFiles(ParamStr(1))
  else
    Writeln('Incorrect number of parameters')
end.
