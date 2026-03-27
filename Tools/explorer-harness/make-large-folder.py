#!/usr/bin/env python3
"""Populate a videos folder with random subfolders and random MP4 copies.

Expected layout in current working directory:
- videos/ directory exists.
- test-1s.mp4 source file exists.
"""

from __future__ import annotations

import shutil
import sys
import random
import uuid
from pathlib import Path


FOLDER_COUNT = 5000
FILES_PER_FOLDER = 4
SOURCE_FILENAME = "test-1s.mp4"
VIDEOS_FOLDER_NAME = "videos"


def find_videos_folder(cwd: Path) -> Path:
    """Return the videos directory under cwd, matching name case-insensitively."""
    matches = [p for p in cwd.iterdir() if p.is_dir() and p.name.lower() == VIDEOS_FOLDER_NAME]
    if not matches:
        print("Error: could not find a videos folder in the current directory.", file=sys.stderr)
        raise SystemExit(1)

    if len(matches) > 1:
        print("Error: found multiple videos folders differing only by case.", file=sys.stderr)
        raise SystemExit(1)

    return matches[0]


def random_name(suffix: str) -> str:
    """Generate a random file or folder name."""
    return f"{uuid.uuid4().hex}{suffix}"


def random_person_name() -> str:
    """Generate an imaginary person name in 'First Last' format."""
    starts = [
        "al", "be", "ca", "da", "el", "fa", "gi", "ha", "io", "ja",
        "ka", "la", "mi", "na", "or", "pa", "qui", "ra", "sa", "ta",
        "ul", "va", "we", "xe", "ya", "za",
    ]
    middles = ["", "n", "r", "l", "m", "v", "d", "s", "th", "ri", "lo", "ni"]
    ends = ["an", "en", "in", "on", "ar", "er", "or", "is", "as", "el", "us", "ia"]

    def build_part() -> str:
        part = f"{random.choice(starts)}{random.choice(middles)}{random.choice(ends)}"
        return part.capitalize()

    first_name = build_part()
    last_name = build_part()
    return f"{first_name} {last_name}"


def main() -> None:
    cwd = Path.cwd() / "tools" / "explorer-harness"
    source_file = cwd / SOURCE_FILENAME
    videos_folder = find_videos_folder(cwd)

    if not source_file.is_file():
        print(f"Error: source file not found: {source_file}", file=sys.stderr)
        raise SystemExit(1)

    existing_folder_count = sum(1 for p in videos_folder.iterdir() if p.is_dir())
    folders_to_create = max(0, FOLDER_COUNT - existing_folder_count)

    created_folders = 0
    created_files = 0

    while created_folders < folders_to_create:
        folder_path = videos_folder / random_person_name()
        if folder_path.exists():
            continue

        folder_path.mkdir(parents=False)
        created_folders += 1

        files_in_this_folder = 0
        while files_in_this_folder < FILES_PER_FOLDER:
            destination = folder_path / random_name(".mp4")
            if destination.exists():
                continue

            shutil.copy2(source_file, destination)
            files_in_this_folder += 1
            created_files += 1

    print(f"Existing folders before run: {existing_folder_count}")
    print(f"Created {created_folders} folders in {videos_folder}")
    print(f"Copied {source_file.name} {created_files} times")


if __name__ == "__main__":
    main()
