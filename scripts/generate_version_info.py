import argparse
import hashlib
import json
from os import listdir, mkdir, path, remove
from shutil import rmtree, copyfile
import subprocess
from tempfile import mkdtemp

class Configuration:

    def __init__(self, path):
        with open(path) as f:
            j = json.load(f)

            self.build_number = j["buildNumber"]
            self.version = j["version"]


def clean_final_output_dir(temp_dir, final_dir):
    # remove those files that are in final_dir that are not in temp_dir
    # these are files that we don't want going forward.

    if not path.isdir(final_dir):
        mkdir(final_dir)
        return

    temp_files = [f for f in listdir(temp_dir) if path.isfile(path.join(temp_dir, f))]
    existing_files = [f for f in listdir(final_dir) if path.isfile(path.join(final_dir, f))]

    del_files = [f for f in existing_files if f not in temp_files]

    for f in del_files:
        remove(f)

def copy_temp_to_final(temp_dir, final_dir):
    # To support incremental builds, only replace files in final_dir
    # if they have truly changed.

    # Generate a checksum for files in temp_dir
    temp_hashes = {}
    for f in listdir(temp_dir):
        fpath = path.join(temp_dir, f)
        if path.isfile(fpath):
            with open(fpath, 'rb') as fr:
                temp_hashes[f] = hashlib.sha1(fr.read()).hexdigest()

    # Generate a checksum for files in final_dir, remove from temp_hashes
    # if equal so we don't copy later.
    for f in listdir(final_dir):
        fpath = path.join(final_dir, f)
        if path.isfile(fpath):
            hash = ''
            with open(fpath, 'rb') as fr:
                hash = hashlib.sha1(fr.read()).hexdigest()

            if hash == temp_hashes[f]:
                del temp_hashes[f]

    if len(temp_hashes) == 0:
        print('    All files up to date.')

    # Copy the files that have changed from temp to final
    for f in temp_hashes.keys():
        tfile = path.join(temp_dir, f)
        ofile = path.join(final_dir, f)

        print(f"    Updating '{f}'")

        remove(ofile)
        copyfile(tfile, ofile)


def get_commit_hash():
    shell = subprocess.run(
        ['git', 'rev-parse', '--short=10', 'HEAD'],
        check=True,
        capture_output=True)

    return shell.stdout.decode('utf-8').strip()

def get_commit_height(config_path):
    change_commit = subprocess.run(
        ['git', 'log', '-n', '1', '--pretty=format:%h', '--', config_path],
        check=True,
        capture_output=True
    ).stdout.decode('utf-8')

    commits_since = subprocess.run(
        ['git', 'rev-list', '--no-merges', '--count', f"{change_commit}..HEAD"],
        check=True,
        capture_output=True
    ).stdout.decode('utf-8')

    # HEAD could have changed the target file, which would make commits_since
    # be 0. The minimum height must be 1. So always add 1 to the result, that
    # way we always return 1 or higher.
    return int(commits_since) + 1

def write_temp_outputs(**kwargs):
    dir = mkdtemp()

    with open(path.join(dir, "version.txt"), 'w') as f:
        f.write(kwargs["version"])

    with open(path.join(dir, "build_number.txt"), 'w') as f:
        f.write(f"{kwargs['build_number']}{(kwargs['height']):03}")

    with open(path.join(dir, "commit_hash.txt"), 'w') as f:
        f.write(kwargs["hash"])

    return dir


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.set_defaults(allow_abbrev=False)

    # positionals
    parser.add_argument(
        'ConfigFile',
        metavar='CONFIG_FILE',
        type=str,
        help='The json file path that contains version settings.'
    )

    parser.add_argument(
        'OutputDir',
        metavar='OUTPUT_DIR',
        type=str,
        help='The directory path to place output data files.'
    )

    args = parser.parse_args()

    config = Configuration(args.ConfigFile)

    commit_hash = get_commit_hash()
    commit_height = get_commit_height(args.ConfigFile)

    temp_dir = write_temp_outputs(
        version=config.version,
        build_number=config.build_number,
        hash=commit_hash,
        height=commit_height
    )

    clean_final_output_dir(temp_dir, args.OutputDir)

    copy_temp_to_final(temp_dir, args.OutputDir)

    rmtree(temp_dir)
