# Initialization

When the repro is used a a template after the initial 
commit the action initialization.yml is executed.
This calls the script `initialize.ps1` in this folder. 

The script replaces placeholders `{{ENVIRONMENT_VARIABLE}}` 
with The content of the environment variable in selected files.

It also installs the main.yml build script, but the script is 
disabled, because there might be changes needed or 
source code first to added to the repro. The branch name of 
`__never__` at the begining of the yaml has to be replaced 
by the actual branch name.