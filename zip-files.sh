#!/bin/bash
cd builds
FOLDERS=*/

for f in $FOLDERS
do
    zip -r "${f%/*}.zip" $f
done

echo "Cleaning up original directories..."
for f in $FOLDERS
do
    rm -R $f
done
echo "Cleaned up. Exiting..."
