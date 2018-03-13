Utilities for The 7th Guest
===========================

Copyright (C) 2000-2006 Deniz Oezmen (http://oezmen.eu/)

All trademarks and copyrights mentioned implicitly or explicitly in this file or
the software described below are property of their respective owners.


T7GGrvEx
--------

Extracts files from RL/GJD archives.

Usage:  T7GGrvEx <RLFile> <GJDFile>

        RLFile          Specifies the RL file to be processed.
        GJDFile         Specifies the matching GJD file.


VDXExt
------

Extracts media content from VDX files.

Usage:  VDXExt <VDXFiles> [/avi] [/sync]

        VDXFiles        Specifies the VDX files to be processed. VDXExt will
                        extract still images, video delta frames and audio files
                        as they appear within each VDX file. Wildcards are
                        allowed.
	avi             Causes VDXExt to convert all frames and an eventually
                        created wave file into an AVI video. The source frames
                        are deleted afterwards. Since the AVI is created using
                        uncompressed RGB images, the creation process is very
                        slow and produces huge amounts of data.
	sync            For synchronisation purposes, you may allow VDXExt to
                        repeat certain audio and/or video frames. This is useful
                        in conjunction with the /avi switch. Note that the end
                        of the audio or video stream will be padded should one
                        of them be longer than the other. Additionally, VDXExt
                        will be caused to create a silent audio stream if none
                        exists in the source VDX file.
