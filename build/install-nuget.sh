#!/usr/bin/env bash

NUGET_URL=https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

SH_SCRIPT_ROOT=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_PATH=$SH_SCRIPT_ROOT/tools
if [ ! -d "$TOOLS_PATH" ]; then
  mkdir "$TOOLS_PATH"
fi

NUGET_PATH=$TOOLS_PATH/nuget.exe
if [ ! -f "$NUGET_PATH" ]; then
    curl -Lsfo "$NUGET_PATH" $NUGET_URL
    if [ $? -ne 0 ]; then
        echo "An error occurred while downloading nuget.exe."
        exit 1
    fi
fi
