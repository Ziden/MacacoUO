

import subprocess


script = "./dropbox_uploader.sh"


def run():
	output, error = bash([script, "list"])
	output = output.split("\n")
	print(output)

def bash(bashCmd):
	process = subprocess.Popen(bashCmd, stdout=subprocess.PIPE)
	return process.communicate()

if __name__ == "__main__":
	run()