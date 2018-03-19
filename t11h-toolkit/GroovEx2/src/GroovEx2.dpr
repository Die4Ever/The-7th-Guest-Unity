{ File    : $URL: file:///D:/SVN/svnDelphi/trunk/11H/GroovEx2/src/GroovEx2.dpr $
  Author  : $Author: Deniz Oezmen $
  Changed : $Date: 2014-07-24 23:41:45 +0200 (Do, 24 Jul 2014) $
  Revision: $Rev: 302 $

  GJD/RL extractor for The 11th Hour.
}
program GroovieExtractor;

{$APPTYPE CONSOLE}

{$R '..\res\version.res' '..\res\version.rc'}

uses
  SysUtils;

const
  MaxGJD        = 255;
  GJDIndex      = 'GJD.GJD';
  DirIndex      = 'DIR.RL';
  GroovieSubDir = 'GROOVIE\';
  MediaSubDir   = 'MEDIA\';
  ROLExt        = 'ROL';
  ROQExt        = 'ROQ';
  RNRExt        = 'RNR';
  XMIExt        = 'XMI';
  HelpParam     = '?';
  _rol          = 1;
  _roq          = 2;
  _rnr          = 4;
  _xmi          = 8;
  _all          = 16;
  _help         = 32;
  _unknown      = 64;

type
  TFileInfo    = record
                   Unknown,
                   Foffs,
                   FSize  : Longint;
                   FIndex : Word;
                   FName  : array[1..18] of Char
                 end;

var
  CDPath       : string;
  GJDIndexFile : file of Char;
  DirFile      : file;
  BundleFiles  : array[1..MaxGJD] of string[12];
  Flags        : Longint;

procedure BuildIndex;
var
  IndexLine: string;
  i        : Integer;
  TempChar : Char;
begin
  Write('Building GJD index... ');
  for i := 1 to MaxGJD do
    BundleFiles[i] := '';
  IndexLine := '';
  TempChar := #000;

  while not Eof(GJDIndexFile) do
  begin
    Read(GJDIndexFile, TempChar);
    if TempChar = #010 then
    begin
      BundleFiles[StrToInt(Copy(IndexLine, Pos(' ', IndexLine) + 1,
        Length(IndexLine) - Pos(' ', IndexLine) + 1))]
        := Copy(IndexLine, 1, Pos(' ', IndexLine) - 1);
      IndexLine := ''
    end
    else
      IndexLine := IndexLine + TempChar
  end;
  Writeln('finished')
end;

procedure ExtractFiles;
var
  FileCount: Integer;
  CurFile,
  CurDest  : file;
  i        : Longint;
  FileInfo : TFileInfo;
  Buf      : array[1..10240] of Char;
begin
  Writeln('Extracting files on this CD... ');
  Writeln;
  FileCount := 0;
  while not Eof(DirFile) do
  begin
    BlockRead(DirFile, FileInfo, SizeOf(FileInfo));
    if (Flags and _all <> 0) or
       ((Flags and _rol <> 0) and (Pos('.' + ROLExt, UpperCase(FileInfo.FName))
         > 0)) or
       ((Flags and _roq <> 0) and (Pos('.' + ROQExt, UpperCase(FileInfo.FName))
         > 0)) or
       ((Flags and _rnr <> 0) and (Pos('.' + RNRExt, UpperCase(FileInfo.FName))
         > 0)) or
       ((Flags and _xmi <> 0) and (Pos('.' + XMIExt, UpperCase(FileInfo.FName))
         > 0))
    then
    begin
      Write('Extracting ', FileInfo.FName, ' from ',
        BundleFiles[FileInfo.FIndex], '... ');
      {$I-}
      Assign(CurFile, CDPath + MediaSubDir + BundleFiles[FileInfo.FIndex]);
      Reset(CurFile, 1);
      {$I+}
      if IOResult <> 0 then
        Writeln('not on this CD')
      else
      begin
        Assign(CurDest, FileInfo.FName);
        Rewrite(CurDest, 1);
        Seek(CurFile, FileInfo.Foffs);
        for i := 1 to FileInfo.FSize div SizeOf(Buf) do
        begin
          BlockRead(CurFile, Buf, SizeOf(Buf));
          BlockWrite(CurDest, Buf, SizeOf(Buf))
        end;
        BlockRead(CurFile, Buf, FileInfo.FSize mod SizeOf(Buf));
        BlockWrite(CurDest, Buf, FileInfo.FSize mod SizeOf(Buf));
        Writeln('finished');
        Close(CurFile);
        Close(CurDest);
        Inc(FileCount)
      end
    end
  end;
  Writeln;
  Writeln(Format('%n files extracted', [FileCount]))
end;

procedure SetFlags;
var
  i          : Integer;
  ParamUpper : string;
begin
  if ParamCount = 1 then
  begin
    Flags := _all;
    if (Copy(ParamStr(1), 1, 1) = '/') or
       (Copy(ParamStr(1), 1, 1) = '-')
    then
    begin
      if UpperCase(Copy(ParamStr(1), 2, Length(ParamStr(1)) - 1)) = HelpParam
        then
        Inc(Flags, _help)
      else
        Inc(Flags, _unknown)
    end
  end
  else
  begin
    Flags := 0;
    for i := 2 to ParamCount do
    begin
      if (Copy(ParamStr(i), 1, 1) = '/') or
         (Copy(ParamStr(i), 1, 1) = '-')
      then
      begin
        ParamUpper := UpperCase(Copy(ParamStr(i), 2, Length(ParamStr(i)) - 1));
        if ParamUpper = ROLExt then
          Inc(Flags, _rol)
        else if ParamUpper = ROQExt
          then
          Inc(Flags, _roq)
        else if ParamUpper = RNRExt
          then
          Inc(Flags, _rnr)
        else if ParamUpper = XMIExt
          then
          Inc(Flags, _xmi)
        else if ParamUpper = HelpParam
          then
          Inc(Flags, _help)
        else if Flags and _unknown = 0 then
          Inc(Flags, _unknown)
      end
      else if Flags and _unknown = 0 then
        Inc(Flags, _unknown)
    end
  end
end;

procedure GetHelp;
begin
  Writeln;
  Writeln('available parameters:');
  Writeln;
  Writeln('/?   - Displays this help screen');
  Writeln('/ROL - Extract .ROL files');
  Writeln('/ROQ - Extract .ROQ files');
  Writeln('/RNR - Extract .RNR files');
  Writeln('/XMI - Extract .XMI files');
  Writeln;
  Writeln('note: By setting and combinating the above switches, you can specifically');
  Writeln('      extract certain file types. By default all files, regardless of their');
  Writeln('      extension, will be extracted')
end;

begin
  Writeln('GrooveEx -- Source file extractor for The Eleventh Hour');
  Writeln;
  SetFlags;
  if (ParamStr(1) = '') then
    Writeln('CD source path not specified')
  else
  begin
    SetFlags;
    if Flags and _help <> 0 then
      GetHelp
    else if (Copy(ParamStr(1), 1, 1) = '/') or
       (Copy(ParamStr(1), 1, 1) = '-') then
      Writeln('CD source path not specified')
    else if Flags and _unknown <> 0 then
      Writeln('Unknown parameter(s) specified in command line')
    else
    begin
      CDPath := ParamStr(1) + '\';
      Write('Opening GJD index file ',
        CDPath + GroovieSubDir + GJDIndex, '... ');
      {$I-}
      Assign(GJDIndexFile, CDPath + GroovieSubDir + GJDIndex);
      Reset(GJDIndexFile);
      {$I+}
      if IOResult <> 0 then
        Writeln('not found')
      else
      begin
        Writeln('found');
        BuildIndex;
        Close(GJDIndexFile);
        Write('Opening directory file ',
          CDPath + GroovieSubDir + DirIndex, '... ');
        {$I-}
        Assign(DirFile, CDPath + GroovieSubDir + DirIndex);
        Reset(DirFile, 1);
        {$I+}
        if IOResult <> 0 then
          Writeln('not found')
        else
        begin
          Writeln('found');
          ExtractFiles;
          Close(DirFile)
        end
      end
    end
  end
end.