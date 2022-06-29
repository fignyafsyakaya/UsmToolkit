#!/bin/bash
rids=( "linux-x64" "linux-musl-x64" "linux-arm" "linux-arm64" "osx-x64" "osx.11.0-arm64" "win-x64" "win-x86" "win-arm" "win-arm64" )
for rid in "${rids[@]}"
do
  echo "Building $rid..."
  dotnet publish -r $rid -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -o "./builds/$rid"
  echo "Finished building $rid"
done
echo "Finishing building all packages"
