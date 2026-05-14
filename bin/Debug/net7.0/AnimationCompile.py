import os
import shutil
import subprocess
import tkinter as tk
from tkinter import filedialog
import sys
import json



class AnimationSetting:
    def __init__(
        self,
        frame_duration=0.0333333333,
        output_name="result.mov",
        output_folder="",
        input_folder="",
        frame_prefix="Frame_",
        frame_digits=5,
        frame_extension="jpg",
        overwrite=True,
    ):
        if not output_name.endswith(".mov"):
            output_name += ".mov"

        self.output_name = output_name
        self.frame_duration = frame_duration
        self.frame_prefix = frame_prefix
        self.frame_digits = frame_digits
        self.frame_extension = frame_extension.lstrip(".")
        self.overwrite = overwrite

        if output_folder == "":
            output_folder = filedialog.askdirectory(title="Select output folder")
        if input_folder == "":
            input_folder = filedialog.askdirectory(title="Select image folder")

        self._output_folder = output_folder
        self._input_folder = input_folder

    @property
    def fps(self):
        if self.frame_duration <= 0:
            raise ValueError("frame_duration must be greater than 0.")
        return 1.0 / self.frame_duration

    @property
    def input_pattern(self):
        return f"{self.frame_prefix}%0{self.frame_digits}d.{self.frame_extension}"

    def get_output_path(self):
        return os.path.join(self._output_folder, self.output_name)

    def set_output_directory(self, output_folder: str):
        self._output_folder = output_folder

    def set_input_directory(self, input_folder: str):
        self._input_folder = input_folder

    def get_input_folder(self):
        return self._input_folder

    def get_output_folder(self):
        return self._output_folder

    def validate(self):
        if not self._input_folder or not os.path.isdir(self._input_folder):
            raise FileNotFoundError(f"Input folder does not exist: {self._input_folder}")
        if not self._output_folder:
            raise FileNotFoundError("Output folder is empty.")
        os.makedirs(self._output_folder, exist_ok=True)

        first_frame = os.path.join(
            self._input_folder,
            f"{self.frame_prefix}{0:0{self.frame_digits}d}.{self.frame_extension}",
        )
        if not os.path.exists(first_frame):
            raise FileNotFoundError(f"First frame does not exist: {first_frame}")

        if shutil.which("ffmpeg") is None:
            raise FileNotFoundError("ffmpeg was not found in PATH.")


class CompileAnimation:
    def __init__(self, setting: AnimationSetting):
        self._setting = setting

    def run(self):
        self._setting.validate()

        output_path = self._setting.get_output_path()
        cmd = [
            "ffmpeg",
        ]

        if self._setting.overwrite:
            cmd.append("-y")

        cmd += [
            "-framerate", str(self._setting.fps),
            "-i", self._setting.input_pattern,
            "-c:v", "libx264",
            "-pix_fmt", "yuv420p",
            "-crf", "18",
            output_path,
        ]

        result = subprocess.run(
            cmd,
            cwd=self._setting.get_input_folder(),
            capture_output=True,
            text=True,
        )

        if result.returncode != 0:
            raise RuntimeError(result.stderr)

        return output_path


def setting_from_json(json_path: str):
    if not json_path:
        raise FileNotFoundError("AnimationSetting json path is empty.")

    json_path = os.path.abspath(os.path.expanduser(json_path))
    print(f"Reading AnimationSetting from: {json_path}")

    if not os.path.exists(json_path):
        raise FileNotFoundError(f"AnimationSetting json file does not exist: {json_path}")

    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    # Some serializers may accidentally save a JSON string instead of a JSON object.
    if isinstance(data, str):
        data = json.loads(data)

    if not isinstance(data, dict):
        raise ValueError(f"AnimationSetting json must be an object, but got: {type(data)}")


    return AnimationSetting(
        frame_duration=data.get("FrameDuration", 0.0333333333),
        output_name=data.get("OutputName", "result.mov"),
        output_folder=data.get("OutputFolder", ""),
        input_folder=data.get("InputFolder", ""),
        frame_prefix=data.get("FramePrefix", "Frame_"),
        frame_digits=data.get("FrameDigits", 5),
        frame_extension=data.get("FrameExtension", "jpg"),
        overwrite=data.get("Overwrite", True),
    )

testpath = "/Volumes/File/IBOIS/Exhibition/01_Animation/P3-2 Wall-H/Data/AnimationSetting.json"

testpathFromCSharp = os.environ.get("ANIMATION_SETTING_JSON", "")

if __name__ == "__main__":
    root = tk.Tk()
    root.withdraw()

    setting = ""


## elif(os.path.isfile(testpath)):
##        setting = setting_from_json(testpath)
##        print("Find json from 3")

    if len(sys.argv) > 1:
        setting = setting_from_json(sys.argv[1])
        print("Find json from 1")
    elif(os.path.isfile(testpathFromCSharp)):
        setting = setting_from_json(testpathFromCSharp)
        print("Find json from 2")
    
    else:
        setting = AnimationSetting()

    compiler = CompileAnimation(setting)
    output = compiler.run()
    print(f"Animation exported: {output}")