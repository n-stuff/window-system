#!/usr/bin/env bash

DOCFX_VERSION=2.47.0

SH_SCRIPT_ROOT=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

. $SH_SCRIPT_ROOT/install-nuget.sh

DOCFX_PATH=$TOOLS_PATH/docfx.console.$DOCFX_VERSION/tools/docfx.exe
if [ ! -f "$DOCFX_PATH" ]; then
    mono "$NUGET_PATH" install docfx.console -Version $DOCFX_VERSION -OutputDirectory "$TOOLS_PATH"
    if [ $? -ne 0 ]; then
        echo "An error occurred while installing DocFX."
        exit 1
    fi
fi
