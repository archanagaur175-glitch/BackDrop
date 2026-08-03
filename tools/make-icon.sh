#!/usr/bin/env bash
# Generates src/BackDrop.App/Assets/BackDrop.ico — a 32x32 32bpp ICO with a
# dark backdrop and a light "play" triangle. Pure byte output, no toolchain.
set -euo pipefail

OUT="src/BackDrop.App/Assets/BackDrop.ico"
mkdir -p "$(dirname "$OUT")"

byte() { printf "\\$(printf '%03o' "$1")"; }

{
  # ---- ICONDIR (6 bytes) ----
  byte 0; byte 0          # reserved
  byte 1; byte 0          # type = icon
  byte 1; byte 0          # count = 1

  # ---- ICONDIRENTRY (16 bytes) ----
  byte 32; byte 32        # width, height
  byte 0; byte 0          # palette, reserved
  byte 1; byte 0          # planes
  byte 32; byte 0         # bpp
  SIZE=$((40 + 4096 + 128))
  byte $((SIZE & 0xFF)); byte $(((SIZE >> 8) & 0xFF)); byte 0; byte 0
  byte 22; byte 0; byte 0; byte 0   # offset to DIB

  # ---- BITMAPINFOHEADER (40 bytes) ----
  byte 40; byte 0; byte 0; byte 0   # biSize
  byte 32; byte 0; byte 0; byte 0   # biWidth
  byte 64; byte 0; byte 0; byte 0   # biHeight (XOR + AND)
  byte 1; byte 0                    # biPlanes
  byte 32; byte 0                   # biBitCount
  byte 0; byte 0; byte 0; byte 0    # biCompression = BI_RGB
  byte 0; byte 16; byte 0; byte 0   # biSizeImage = 4096
  byte 0; byte 0; byte 0; byte 0    # biXPels
  byte 0; byte 0; byte 0; byte 0    # biYPels
  byte 0; byte 0; byte 0; byte 0    # biClrUsed
  byte 0; byte 0; byte 0; byte 0    # biClrImportant

  # ---- XOR bitmap: 32 rows, bottom-up, BGRA ----
  for ((y = 31; y >= 0; y--)); do
    for ((x = 0; x < 32; x++)); do
      inside=0
      if (( y >= 8 && y <= 24 )); then
        if (( y <= 16 )); then
          right=$((10 + (y - 8) * 3 / 2))
        else
          right=$((22 - (y - 16) * 3 / 2))
        fi
        if (( x >= 10 && x <= right )); then inside=1; fi
      fi
      if (( inside )); then
        byte 0xF6; byte 0xEA; byte 0xE8; byte 0xFF   # light glyph
      else
        byte 0x18; byte 0x12; byte 0x10; byte 0xFF   # dark bg
      fi
    done
  done

  # ---- AND mask: 128 bytes, opaque ----
  for ((i = 0; i < 128; i++)); do byte 0; done
} > "$OUT"

echo "wrote $OUT ($(wc -c < "$OUT") bytes)"
