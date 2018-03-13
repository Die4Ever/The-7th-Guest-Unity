{ File   : T7GGrvEx.dpr
  Author : Deniz Oezmen
  Created: 2006-03-18, based on T7GGRVEX.PAS created ~2000
  Changed: 2006-03-18

  GJD/RL extractor for The 7th Guest.
}
program T7GGroovieExtractor;

{$APPTYPE CONSOLE}

uses
  SysUtils;

type
  TFileInfo = packed record
                FName: array[0..11] of Char;
                FOffs,
                FSize: Longint
              end;

var
  GJDName,
  RLName : string;
  RLFile : file of TFileInfo;
  GJDFile: file;

function CheckConsistency: Boolean;
var
  FileInfo: TFileInfo;
  NumFiles: Longint;
begin
  Result := False;

  NumFiles := FileSize(RLFile);
  if NumFiles < 1 then
    Exit;

  Seek(RLFile, NumFiles - 1);
  Read(RLFile, FileInfo);

  Result := (FileInfo.FOffs + FileInfo.FSize + 1 = FileSize(GJDFile))
end;

procedure ExtractAll;
var
  i,
  FileCount: Integer;
  FileInfo : TFileInfo;
  FName    : string;
  Buf      : array[1..10240] of Char;
  DestFile : file;
begin
  FileCount := 0;

  Reset(RLFile);
  FileMode := fmOpenWrite;

  while not EOF(RLFile) do
  begin
    Read(RLFile, FileInfo);

    FName := FileInfo.FName;
    FName := Copy(FName, 1, StrLen(PChar(FName)));

    Write('Extracting ', FName, ' ... ');

    {$I-}
    Assign(DestFile, FName);
    Rewrite(DestFile, 1);
    {$I+}
    if IOResult <> 0 then
    begin
      WriteLn('error writing to output file');
      Continue
    end;

    Seek(GJDFile, FileInfo.FOffs);
    for i := 1 to FileInfo.FSize div SizeOf(Buf) do
    begin
      BlockRead(GJDFile, Buf, SizeOf(Buf));
      BlockWrite(DestFile, Buf, SizeOF(Buf))
    end;
    BlockRead(GJDFile, Buf, FileInfo.FSize mod SizeOf(Buf));
    BlockWrite(DestFile, Buf, FileInfo.FSize mod SizeOF(Buf));

    Close(DestFile);
    WriteLn('finished');
    Inc(FileCount)
  end;
  
  WriteLn(Format('%d files extracted', [FileCount]))
end;

begin
  if ParamCount <> 2 then
  begin
    WriteLn('Usage: T7GGRVEX <rl-file> <gjd-file>');
    Exit
  end;

  RLName := ParamStr(1);
  GJDName := ParamStr(2);

  FileMode := fmOpenRead;
  {$I-}
  Assign(RLFile, RLName);
  Reset(RLFile);
  {$I+}
  if IOResult <> 0 then
  begin
    WriteLn(RLName, ' not found');
    Exit
  end;

  {$I-}
  Assign(GJDFile, GJDName);
  Reset(GJDFile, 1);
  {$I+}
  if IOResult <> 0 then
  begin
    WriteLn(GJDName, ' not found');
    Exit
  end;

  if not CheckConsistency then
  begin
    WriteLn('Directory file doesn''t match bundle file');
    Exit
  end;

  ExtractAll;

  Close(RLFile);
  Close(GJDFile)
end.
