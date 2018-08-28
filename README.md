## Join CSharp

Simple, Roslyn-based tool to join a bunch of .cs files into one.

## Installation

The tool can be installed from nuget as a .Net Core global tool as follows:

    dotnet tool install -g Join.CSharp

## Usage

    joincs inputfolder [outputfile] [<comma-separated list of preprocessor directives>]

If no output file is specified, the result is written to the console.

## Known issues

Preprocessor directives and comments are stripped from using statements, namespaces and top-level class definitions.
