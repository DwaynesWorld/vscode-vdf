# VDF support for Visual Studio Code [![Build Status](https://travis-ci.com/DwaynesWorld/vscode-vdf.svg?branch=master)](https://travis-ci.com/DwaynesWorld/vscode-vdf)

Adds language support for VDF to Visual Studio Code.

Supports:

- code completion
- jump to definition, peek definition
- code formatting
- snippets
- syntax highlighting

## TODO

- ~~Get Unit Testing and Travis CI setup~~
- error squiggles and apply suggestions from errors
- ~~types and documentation on hover~~
- find all references
- ~~symbol search~~
- build tasks
- refactoring (rename, extract to method)
- Lots more...

## Known issues

- ~~regex needs some help, does not find methods with array brackets in declaration correctly~~

## Contributing

### Get Started

Install the dependencies and devDependencies.

```sh
$ yarn
```

Build for debugging

```sh
$ yarn build
```

Or Build for Release

(Windows)

```sh
$ yarn package-win
```

(OSX)

```sh
$ yarn package-osx
```

Install extension locally (after running yarn and yarn package~)

```sh
$ yarn install-ext
```
