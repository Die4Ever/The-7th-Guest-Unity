Utilities for The 11th Hour
===========================

Copyright (C) 2000-2014 Deniz Oezmen (http://oezmen.eu/)

All trademarks and copyrights mentioned implicitly or explicitly in this file or
the software described below are property of their respective owners.


GroovEx2
--------

Extracts files from the GJD/RL archives.

Usage:  GroovEx2 <CD-Path> [/?] [/ROL] [/ROQ] [/RNR] [/XMI]

        CD-Path         Specifies the path of one of the 11th Hour CDs. The
                        subdirectories \MEDIA and \GROOVIE are expected to be
                        located in this path.
        /?              Shows a short help screen.
        /ROL            Extracts only ROL files.
        /ROQ            Extracts only ROQ files.
        /RNR            Extracts only RNR files.
        /XMI            Extracts only XMI files.

        The switches /ROL, /ROQ, /RNR and /XMI can be combined. If none of them
        are specified, all files on the given CD are extracted.


RExt
----

Extracts and converts media from ROL/ROQ/RNR files.

Usage:  RExt <RFiles>

        RFiles           Specifies the files to be processed. Wildcards are
                         allowed.

        Currently, only the audio stream (which will be saved as a standard PCM
        wave file) and some JPEG still frames can be extracted by this program.
