# The-7th-Guest-Unity
a remake of The 7th Guest in Unity, with options to upscale the original videos and gameplay options

did some upscaling tests here https://www.youtube.com/playlist?list=PLZIQTa_kwZhDhJiNmrEGf2kTPLnKfdE0e

using ffmpeg's minterpolate option for now https://ffmpeg.org/ffmpeg-filters.html#minterpolate
ffmpeg -i "f6_1.avi" -b:a 256k -b:v 20M -filter_complex "[0:v]minterpolate='fps=60:mi_mode=mci:scd=none:vsbmc=1', xbr=4[v];[0:a]acopy[a]" -map "[v]" -map "[a]" "f6_1_60fps.avi"
feel free to suggest a better method if you know one

http://wiki.xentax.com/index.php/The_7th_Guest_Toolset

http://wiki.xentax.com/index.php/The_7th_Guest_VDX

http://www.mirsoft.info/gmb/music_info.php?id_ele=MTI3NQ==

Also need to test out Waifu2x upscaler
