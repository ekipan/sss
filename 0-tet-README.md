(TODO screenshots)

A 50fps Tetris in Forth for the Commodore 64. _(No sound.)_

## Trying It Out

It's probably easiest to load the [durexForth][dur] cartridge
or disk into a [Commodore 64 emulator][vic] then paste the
program directly. You could also get it onto a disk file then
type `include tet.fs`. durexForth has a vi clone if you want
to make a file that way: `v tet.fs<return>i<alt-insert>~ZZ`
to paste, save, and exit in the usual VICE config.

After compiling, type `help` to see keys, `new` to play,
`1 prof 123 init r` then framestep to [measure time][per].

[src]: ./tet.fs
[des]: ./DESIGN.md

## Further Reading

If you're interested in learning how it works, start with the
[design tour][des]. The main source [tet.fs][src] is extremely
dense, as its intended audience is just myself. The tour,
however, aims for less longbearded folk, with overview,
implementation detail, tradeoff reflections, etc. Keep both
open for cross-reference.

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/
[per]: ./DESIGN.md#performance-and-tradeoffs

<!-- end of README.md -->

