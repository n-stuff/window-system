#!/usr/bin/env bash

SH_SCRIPT_ROOT=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

. $SH_SCRIPT_ROOT/install-docfx.sh

REPOSITORY_PATH=$( cd $SH_SCRIPT_ROOT && cd .. && pwd )

exec mono $DOCFX_PATH $REPOSITORY_PATH/docs/docfx.json
