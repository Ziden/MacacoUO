

import subprocess


script = "./dropbox_uploader.sh"


def run():

	output, error = bash([script, "upload ~/t2a/UF2/UltimaFronteira/Backups/Automatic", "/Ultimo"])


	output, error = bash([script, "list"])
	output = output.split("\n")

	ct = output.length
	for linha in output:
		if "Ultimo" in linha:
			bash([script, "move Ultimo Bkp_{}".format(ct), "/UltimoBackup"])

	print(output)

def bash(bashCmd):
	process = subprocess.Popen(bashCmd, stdout=subprocess.PIPE)
	return process.communicate()

if __name__ == "__main__":
	run()