"""
Levels every sound so none is much louder or quieter than the rest.

Originals live in Assets/SoundSources (never edited); the levelled copies
the game plays are written to Assets/Sounds. Needs ffmpeg on the PATH.

    python Tools/normalize_sounds.py

Three groups, each brought to its own target:
  effects  - short sounds: their loudest moment (RMS peak, 50 ms) to
             EFFECT_TARGET dB, then a limiter keeps peaks below -1 dBFS
  loops    - rain_loop, hall_ambience: integrated loudness LOOP_LUFS
             (a quiet bed under everything)
  music    - Title_music: integrated loudness MUSIC_LUFS
To add a sound: put the original in Assets/SoundSources and rerun.
"""
import re
import subprocess
from pathlib import Path

SRC = Path("Assets/SoundSources")
DST = Path("Assets/Sounds")
EFFECT_TARGET = -14.0
LOOP_LUFS = -30.0
MUSIC_LUFS = -18.0
LOOPS = {"rain_loop", "hall_ambience"}
MUSIC = {"title_music"}


def ff(args):
    return subprocess.run(["ffmpeg", "-hide_banner", "-nostats", *args],
                          capture_output=True, text=True).stderr


def rms_peak(path):
    out = ff(["-i", str(path), "-af", "astats=metadata=0:measure_perchannel=none:length=0.05", "-f", "null", "-"])
    return float(re.search(r"RMS peak dB:\s*(-?[\d.]+|-inf)", out).group(1))


def lufs(path):
    out = ff(["-i", str(path), "-af", "ebur128", "-f", "null", "-"])
    return float(re.findall(r"I:\s*(-?[\d.]+) LUFS", out)[-1])


def write(src, dst, gain_db):
    limiter = "alimiter=limit=0.891:attack=2:release=60:level=false"
    codec = ["-c:a", "libmp3lame", "-q:a", "2"] if dst.suffix == ".mp3" else ["-c:a", "pcm_s16le"]
    ff(["-y", "-i", str(src), "-af", f"volume={gain_db:.2f}dB,{limiter}", *codec, str(dst)])


for src in sorted(SRC.iterdir()):
    if src.suffix.lower() not in (".wav", ".mp3"):
        continue
    name = src.stem.lower()
    if name in LOOPS:
        gain = LOOP_LUFS - lufs(src)
        group = "loop"
    elif name in MUSIC:
        gain = MUSIC_LUFS - lufs(src)
        group = "music"
    else:
        gain = EFFECT_TARGET - rms_peak(src)
        group = "effect"
    write(src, DST / src.name, gain)
    # The limiter can hold back a sound with a sharp spike: measure the
    # result and top it up (up to twice) until it reaches the target.
    for _ in range(2):
        if group != "effect":
            break
        short = EFFECT_TARGET - rms_peak(DST / src.name)
        if short < 1.0:
            break
        gain += short
        write(src, DST / src.name, gain)
    print(f"{src.name:22} {group:7} {gain:+6.1f} dB")
