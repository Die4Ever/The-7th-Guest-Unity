{ File   : SaveBMP2.pas
  Author : Deniz Oezmen, based on work by Christian Klaessen and Tobias Post
  Created: 1999-12-11
  Changed: 2005-10-01

  Library providing various functions to export data as a standard Windows
  bitmap file.
}
unit SaveBMP2;

interface

  const
    Version = $0501;
    Build   = $0000;
    Author  = 'T. Post, C. Klaessen and D. Oezmen, 1999-2005';

  type
    TColor = packed record
               B, G, R, A: Byte
             end;

    TColor3 = packed record
                B, G, R: Byte;
              end;

    TPalette = packed array[0..255] of TColor;

    TPalette3 = packed array[0..255] of TColor3;


    TBitmapFileHeader = packed record
                         bfType     : Word;    {UINT  : Erkennungsstring = 'BM'}
                         bfSize     : Longint; {DWORD : Gr”áe der Bitmapdatei}
                         bfReserved1,          {UINT  : Reserviert1 = 0}
                         bfReserved2: Word;    {UINT  : Reserviert2 = 0}
                         bfOffBits  : Longint  {DWORD : Beginn (Offset) der Bilddaten}
                        end;

    TBitmapInfoHeader = packed record
                          biSize,                   {DWORD : Gr”áe des BMIH}
                          biWidth,                  {LONG  : Breite des Bildes}
                          biHeight       : Longint; {LONG  : H”he des Bildes}
                          biPlanes,                 {WORD  : Bildebenen? = 1}
                          biBitCount     : Word;    {WORD  : Bits pro Pixel = 1/4/8/24}
                          biCompression,            {DWORD : Komprimierung = 0/1}
                          biSizeImage,              {DWORD : Gr”áe der Bilddaten}
                          biXPelsPerMeter,          {LONG  : X-Pixelanzahl pro Meter}
                          biYPelsPerMeter,          {LONG  : Y-Pixelanzahl pro Meter}
                          biClrUsed,                {DWORD : Farbanzahl in Farbpalette}
                          biClrImportant : Longint  {DWORD : Bits Pro Pixel des Grafiktreibers}
                        END;

     TBMPFile         = packed record
                          BMPFile   : file;
                          FileHeader: TBitmapFileHeader;
                          InfoHeader: TBitmapInfoHeader
                        end;

  procedure OpenBMP(var BMP: TBMPFile; BMPName: string; XRes, YRes: Longint);
  procedure WritePixelToBMP(BMP: TBMPFile; x, y: Longint; R, G, B: Byte);
  procedure CloseBMP(BMP: TBMPFile);
  procedure Dump8BitBMP(BMPName: string; XRes, YRes: Longint; Palette: TPalette;
    Content: Pointer); overload;
  procedure Dump8BitBMP(BMPName: string; XRes, YRes: Longint;
    Palette3: TPalette3; Content: Pointer); overload;

implementation

procedure PrepareBMP(var BMP: TBMPFile; BMPName: string; XRes, YRes: Longint;
  Bits: Word);
var
  ScanX,
  ScanY     : Longint;
begin
  Assign(BMP.BMPFile, BMPName);
  Rewrite(BMP.BMPFile, 1);

  ScanX := XRes;
  ScanY := YRes;

  ScanX := (ScanX div 4 + Byte(ScanX mod 4 > 0)) * 4;

  with BMP.FileHeader do
  begin
    bfType := Ord('B') + Ord('M') * 256;
    bfSize := SizeOf(BMP.FileHeader) + SizeOf(BMP.InfoHeader) +
      3 * ScanX * ScanY;
    bfReserved1 := $00;
    bfReserved2 := $00;
    bfOffBits := SizeOf(BMP.FileHeader) + SizeOf(BMP.InfoHeader)
  end;

  with BMP.InfoHeader do
  begin
    biSize := SizeOf(BMP.InfoHeader);
    biWidth := XRes;
    biHeight := YRes;
    biPlanes := $01;
    biBitCount := Bits;
    biCompression := $0000;
    biSizeImage := 3 * ScanX * ScanY;
    biXPelsPerMeter := $0000;
    biYPelsPerMeter := $0000;
    biClrUsed := $0000;
    biClrImportant := $0000
  end;

  BlockWrite(BMP.BMPFile, BMP.FileHeader, SizeOf(BMP.FileHeader));
  BlockWrite(BMP.BMPFile, BMP.InfoHeader, SizeOf(BMP.InfoHeader))
end;

procedure Prepare8BitBMP(var BMP: TBMPFile; BMPName: string;
  XRes, YRes: Longint; Bits: Word);
var
  ScanX,
  ScanY     : Longint;
begin
  Assign(BMP.BMPFile, BMPName);
  Rewrite(BMP.BMPFile, 1);

  ScanX := XRes;
  ScanY := YRes;

  ScanX := (ScanX div 4 + Byte(ScanX mod 4 > 0)) * 4;

  with BMP.FileHeader do
  begin
    bfType := Ord('B') + Ord('M') * 256;
    bfSize := SizeOf(BMP.FileHeader) + SizeOf(BMP.InfoHeader) +
      SizeOf(TPalette) + ScanX * ScanY;
    bfReserved1 := $00;
    bfReserved2 := $00;
    bfOffBits := SizeOf(BMP.FileHeader) + SizeOf(BMP.InfoHeader) +
      SizeOf(TPalette);
  end;

  with BMP.InfoHeader do
  begin
    biSize := SizeOf(BMP.InfoHeader);
    biWidth := XRes;
    biHeight := YRes;
    biPlanes := $01;
    biBitCount := Bits;
    biCompression := $0000;
    biSizeImage := 3 * ScanX * ScanY;
    biXPelsPerMeter := $0000;
    biYPelsPerMeter := $0000;
    biClrUsed := $0000;
    biClrImportant := $0000
  end;

  BlockWrite(BMP.BMPFile, BMP.FileHeader, SizeOf(BMP.FileHeader));
  BlockWrite(BMP.BMPFile, BMP.InfoHeader, SizeOf(BMP.InfoHeader))
end;

procedure FillBMP(BMP: TBMPFile);
var
  i     : Longint;
  Buffer: array[0..10239] of Byte;
begin
  for i := 0 to SizeOf(Buffer) - 1 do
    Buffer[i] := 0;

  for i := 1 to BMP.InfoHeader.biSizeImage div SizeOf(Buffer) do
    BlockWrite(BMP.BMPFile, Buffer, SizeOf(Buffer));
  BlockWrite(BMP.BMPFile, Buffer,
    BMP.InfoHeader.biSizeImage mod SizeOf(Buffer))
end;

procedure OpenBMP(var BMP: TBMPFile; BMPName: string; XRes, YRes: Longint);
begin
  PrepareBMP(BMP, BMPName, XRes, YRes, 24);
  FillBMP(BMP)
end;

procedure Dump8BitBMP(BMPName: string; XRes, YRes: Longint; Palette: TPalette;
  Content: Pointer);
var
  BMP  : TBMPFile;
  Line : Pointer;
  y,
  Width: Word;
begin
  Prepare8BitBMP(BMP, BMPName, XRes, YRes, 8);

  BlockWrite(BMP.BMPFile, Palette, SizeOf(Palette));

  Width := BMP.InfoHeader.biWidth;
  Width := (Width div 4 + Byte(Width mod 4 > 0)) * 4;

  GetMem(Line, Width);
  FillChar(Line^, Width, 0);

  for y := BMP.InfoHeader.biHeight - 1 downto 0 do
  begin
    Move(Pointer(Longint(Content) + (y * BMP.InfoHeader.biWidth))^,
      Line^, BMP.InfoHeader.biWidth);
    BlockWrite(BMP.BMPFile, Line^, Width)
  end;

  FreeMem(Line, Width);

  CloseBMP(BMP)
end;

procedure ConvertPalette3toPalette(Palette3: TPalette3; var Palette: TPalette);
var
  i: Byte;
begin
  for i := 0 to 255 do
  begin
    Palette[i].B := Palette3[i].B;
    Palette[i].G := Palette3[i].G;
    Palette[i].R := Palette3[i].R;
    Palette[i].A := 0
  end
end;

procedure Dump8BitBMP(BMPName: string; XRes, YRes: Longint; Palette3: TPalette3;
  Content: Pointer);
var
  Palette: TPalette;
begin
  ConvertPalette3ToPalette(Palette3, Palette);
  Dump8BitBMP(BMPName, XRes, YRes, Palette, Content)
end;

procedure WritePixelToBMP(BMP: TBMPFile; x, y: Longint; R, G, B: Byte);
var
  Color: record
           B, G, R: Byte
         end;
  XRes,
  YRes : Longint;
begin
  Color.R := R;
  Color.G := G;
  Color.B := B;

  XRes := Longint(BMP.InfoHeader.biWidth);
  YRes := Longint(BMP.InfoHeader.biHeight);

  Seek(BMP.BMPFile, $0036 + (YRes - y - 1) * 3 * XRes +
        (YRes - y - 1) * (XRes mod 4) + 3 * x);
  BlockWrite(BMP.BMPFile, Color, SizeOf(Color))
end;

procedure CloseBMP(BMP : TBMPFile);
begin
  Close(BMP.BMPFile)
end;

begin
end.
