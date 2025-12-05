# SSS: The Silent Soviet Stacker

A block-stacking game for the Commodore 64. Written in the
number-stacking language Forth and achieving mostly full
50fps, but no sound.

<!-- TODO after migrating from Gist to proper repo:
![Game screenshots.](./screenshot.png)
-->

## Trying It Out

It's probably easiest to load the [durexForth][dur] cartridge
or disk into a [Commodore 64 emulator][vic] then paste the
program directly. You could also get it onto a disk file then
type `include tet.fs`. durexForth has a vi clone if you want
to make a file that way: `v tet.fs<return>i<alt-insert>~ZZ`
to paste, save, and exit in the usual VICE config.

After compiling, type `help` to see keys, `new` to play,
`1 prof 123 init r` then framestep to [measure time][per].

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/
[per]: #performance-and-tradeoffs

## Further Reading

If you're interested in learning how it works, start with the
[design tour][des] and cross-reference [the source][src].

[src]: #file-2-tet-fs
[des]: #file-1-design-md

<!-- end of README.md -->
