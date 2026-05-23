#!/bin/sh
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Projects in publish order (Archivarius first)
PROJECTS=(
    "Archivarius"
    "Archivarius.Extensions"
    "Archivarius.Json"
    "Archivarius.Tools"
)

for project in "${PROJECTS[@]}"; do
    bin_dir="$REPO_ROOT/$project/bin"
    nupkg=$(find "$bin_dir" -name "*.nupkg" ! -name "*.snupkg" -type f 2>/dev/null | xargs ls -t 2>/dev/null | head -1 || true)

    if [[ -z "$nupkg" ]]; then
        echo "SKIP: $project — no .nupkg found"
        continue
    fi
    
    echo "$nupkg"

    echo "Pushing $project: $(basename "$nupkg")"
    nuget push "$nupkg" -Source https://api.nuget.org/v3/index.json -SkipDuplicate

    snupkg="${nupkg%.nupkg}.snupkg"
    if [[ -f "$snupkg" ]]; then
        echo "Pushing symbols: $(basename "$snupkg")"
        nuget push "$snupkg" -Source https://api.nuget.org/v3/index.json -SkipDuplicate
    fi

    echo "Cleaning $project packages..."
    find "$bin_dir" -name "*.nupkg" -o -name "*.snupkg" | xargs rm -f
done

echo "Done."
