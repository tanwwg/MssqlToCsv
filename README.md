# MssqlToCsv

## Usage

    MssqlToCsv --server <server> --user <user> --catalog <db> --query "select * from X" --output x.tsv

## Building

    dotnet publish -r osx.11.0-x64 -c Release --self-contained true -p:PublishReadyToRun=true

