import os
import tkinter as tk
from tkinter import filedialog
import sys
import subprocess
from subprocess import CalledProcessError

root = tk.Tk()
root.withdraw()

def do_program():
    project_path = os.getcwd()
    print(f""" {Color.CYAN}@@@@@@@@    @@@@@@@@@   @@@@@@@@@    @@@@   @@@@         @@@@@@@@                                       
 @@@@@@@@@@  @@@@@@@@@   @@@@@@@@@@@   @@@   @@@@        @@@@@@@@@                                       
 @@@   @@@@  @@@@        @@@@   @@@@   @@@   @@@@        @@@@              @@@@     @@@@  @@@  @@@@@@@@@ 
 @@@   @@@@  @@@@@@@     @@@@@@@@@@@   @@@   @@@@        @@@@@@@@@        @@@@@@    @@@@@ @@@  @@@   @@@ 
 @@@@@@@@@   @@@@        @@@@@@@@@     @@@   @@@@             @@@@@       @@@@@@    @@@@@@@@@  @@@   @@@ 
 @@@         @@@@@@@@@@  @@@@ @@@@@    @@@   @@@@@@@@@@ @@@@@@@@@@@      @@@@@@@@   @@@ @@@@@  @@@   @@@ 
 @@@@        @@@@@@@@@@  @@@@   @@@@  @@@@   @@@@@@@@@@  @@@@@@@@       @@@    @@@  @@@  @@@@  @@@@@@@@  
                                                                                                         
                                                                                                         
 @@@@@@@@@@    @@@@@  @@@@@@@@@@@@@@  @@@@@@@@@@@    @@@@@       @@@@@          @@@@          @@@@@@@@@@ 
 @@@@@@@@@@@@  @@@@@  @@@@@@@@@@@@@@  @@@@@@@@@@@   @@@@@@@       @@@@          @@@@         @@@@@@@@@@@ 
 @@@@   @@@@@  @@@@@       @@@@       @@@@          @@@@@@@@      @@@@          @@@@         @@@@@       
 @@@@   @@@@@  @@@@@       @@@@       @@@@@@@@     @@@@ @@@@      @@@@          @@@@         @@@@@@@@@@  
 @@@@@@@@@@@   @@@@@       @@@@       @@@@@@@@     @@@@ @@@@@     @@@@          @@@@             @@@@@@@@
 @@@@@@@@@@    @@@@@       @@@@       @@@@        @@@@@@@@@@@     @@@@          @@@@       @         @@@@
 @@@@          @@@@@       @@@@       @@@@       @@@@@@@@@@@@@    @@@@@@@@@@@   @@@@@@@@@@@  @@@@@@@@@@@@
@@@@@          @@@@@      @@@@@       @@@@@     @@@@@     @@@@@  @@@@@@@@@@@@  @@@@@@@@@@@@  @@@@@@@@@@  


        """)

    print(f"{Color.GREEN}Welcome to the aforementioned build utility. Guaranteed to work just enough but not too well.")
    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}I'll walk you through the steps so I can get back to more...er..important things.")
    unity_version = ""
    try:    
        f = open(f"{project_path}\\ProjectSettings\\ProjectVersion.txt", "r")
    
        unity_version = f.readline()[17:].strip()
        f.close()
    except OSError:
        print(f"{Color.RED}[THE KERNEL, RING 0] Hey, user. I hit a snag on this one and could not read ProjectSettings/ProjectVersion.txt. Are you running this from the Unity project directory?")
        print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}He makes the rules. Try again later.")
        return 1

    print("\n")

    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}Searching for Unity version matching {Color.MAGENTA}{unity_version}{Color.WHITE}...")
    unity_path = rf"C:\Program Files\Unity\Hub\Editor\{unity_version}\Editor\Unity.exe"
    if os.path.exists(unity_path):
        print(f"{Color.GREEN}[BUILDER] {Color.WHITE}Unity found! And I found it by myself, no thanks to you...")
    else:
        print(f"{Color.RED}[BUILDER] {Color.WHITE}BLAST! I could not find a Unity install matching {Color.MAGENTA}{unity_version}{Color.WHITE}. Lend me a hand, won't you?")
        unity_path = filedialog.askopenfilename(title="Please find Unity for me...", multiple="False", initialdir="C:\\Program Files\\Unity\\Hub\\Editor", filetypes=[("Unity Editor", "Unity.exe")])

    print("\n")

    # if dog, random.dog
    print(f"{Color.YELLOW}[PERSISTENT MEMORY] {Color.WHITE}{Color.UNDERLINE}Wait! Make sure you've set your output folder to be on the right Git branch.{Color.RESET}")
    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}Right -- what flavor of build would you like to make?")

    build_opt = input(f"""
1. Dev (Uncompressed - for GitHub site)
2. Dev (Compressed - for SimCase site)
3. Release (For ACTUAL releases)

{Color.YELLOW}Type your choice [press ENTER for 1]:{Color.RESET}   """).strip()

    if build_opt == "": build_opt = "1"
    if build_opt not in ["1", "2", "3"]:
        print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}I didn't expect much of you, but frankly I have no idea how you messed that up. I'm done here.")
        print(f"{Color.YELLOW}[PERSISTENT MEMORY] {Color.WHITE}It's okay. Just remember: hit 1, 2, or 3, and then hit ENTER.")
        return 1 

    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}Final question: dog mode? [y/N]")
    dog_opt = input()
    dog_opt = True if "y" in dog_opt else False

    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}Waiting for you to select an output directory...")
    output_path = filedialog.askdirectory()

    print(f"{Color.YELLOW}[BUILDER] {Color.WHITE}Passing off to Unity for building. I'll let you know when it succeeds or fails.")
    try:
        if (build_opt == "1" or build_opt == ""):
            subprocess.check_output([unity_path, "-batchmode", f"-projectPath \"{project_path}\"", "-buildTarget webgl", "-executeMethod", "BatchBuild.DoBuild", "-outputPath", output_path, "-quit", "-logfile"])
        elif (build_opt == "2"):
            subprocess.check_output([unity_path, "-batchmode", f"-projectPath \"{project_path}\"", "-buildTarget webgl", "-executeMethod", "BatchBuild.DoBuildCompressed", "-outputPath", output_path, "-quit", "-logfile"])
        elif (build_opt == "3"):
            subprocess.check_output([unity_path, "-batchmode", f"-projectPath \"{project_path}\"", "-buildTarget webgl", "-executeMethod", "BatchBuild.DoBuildRelease", "-outputPath", output_path, "-quit", "-logfile"])
    except CalledProcessError as e:
        print(f"{Color.RED}[BUILDER] {Color.WHITE}The computer has failed you. I don't cast blame -- here's what the program said:")
        print(f"({e.returncode}) {e.output}")
        return 1

    print(f"{Color.GREEN}[BUILDER] {Color.WHITE}SUCCESS! Enjoy that. We worked hard for it.")
    print(f"{Color.GREEN}[BUILDER] {Color.WHITE}Until next time, {os.getlogin()}...")


def do_program_wrap():
    try:
        do_program()
    except Exception as e:
        print(f"{Color.RED}[BUILDER] {Color.WHITE}Hey, I did warn you it wouldn't work *too* well.")
        print(e)

    input("Press ENTER to close this window...")



class Color():
    BLACK = '\033[30m'
    RED = '\033[31m'
    GREEN = '\033[32m'
    YELLOW = '\033[33m'
    BLUE = '\033[34m'
    MAGENTA = '\033[35m'
    CYAN = '\033[36m'
    WHITE = '\033[37m'
    UNDERLINE = '\033[4m'
    RESET = '\033[0m'

if __name__ == "__main__":
    do_program_wrap()