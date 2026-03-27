#!/usr/bin/env python3
# Create a short test video by calling ffmpeg from the command line.
# Assumes ffmpeg is available in PATH.
from __future__ import annotations

import subprocess
import sys
from pathlib import Path

def create_video(output: Path) -> None:
	"""Generate a 1-second MP4 video with arbitrary visual content."""
	command = [
		"ffmpeg",
		"-y",
		"-f",
		"lavfi",
		"-i",
		"testsrc=size=320x240:rate=15",
		"-t",
		"1",
		"-pix_fmt",
		"yuv420p",
		str(output),
	]

	try:
		subprocess.run(command, check=True)
	except FileNotFoundError:
		print("Error: ffmpeg was not found in PATH.", file=sys.stderr)
		raise SystemExit(1)
	except subprocess.CalledProcessError as exc:
		print(f"Error: ffmpeg failed with exit code {exc.returncode}.", file=sys.stderr)
		raise SystemExit(exc.returncode)


def main() -> None:
	output = "test-1s.mp4"
	create_video(output)
	print(f"Created: {output}")


if __name__ == "__main__":
	main()
