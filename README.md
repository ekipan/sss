<!-- markdownlint-disable no-trailing-punctuation -->

# SSS: The Silent Soviet Stacker

A block-stacking game written in the number-stacking language
Forth, for the Commodore 64. Pause a game in progress then
tinker with the live game state in the interpreter.

| [Explore the design][des] | [Start tinkering][tin]      |
|---------------------------|-----------------------------|
| ![Game in progress][p]    | ![Forth interpreter log][d] |

[p]: shots/play.png
[d]: shots/devel.png

## Try It Out
<!---------->

1. Get the [VICE][vic] C64 emulator.
2. Load the [durexForth][dur] cart or disk.
   [SSS is developed in v4][hfs] but sometimes I also test v5.
3. Copy [`sss.fs`][sss] contents then Edit > Paste in VICE.
   Takes a minute to compile.
4. Type `help` then `new`.

> [!WARNING]
> Not a product for general audiences: [No sound][sou],
> [no high scores][sco], no menus, and weird controls:

### Keys

- **S F** - Shift left and right.
- **E D** - Hard drop, soft drop.
- **J K** - Rotate left and right.
- **L** - Store piece in hold slot on bottom right.
- Anything else - Pause game and return to Forth.

My own strong preference. See input rationale for a(n
admittedly not easy) [reconfiguration option][inp].

## This thing:
<!----------->

- **Is my personal code garden** optimized to **bring me
  joy,** which among other things means a 50fps target with as
  little assembly code as necessary, because Forth is fun!
- **Explores creative constraints.** durexForth is
  _fast,_ but not as fast as hand-rolled 6502. Arbitrary
  [source text constraints][den], too. They're my muse.
- **Is a document suite,** because I think it:
- **Wants to be [tinkered with][tin],** so it:
- **Might motivate the Forth- or C64-curious,** but:
- **Is NOT a Tetris product!** No sound, no menus, weird keys.
  General audiences will balk, but I play until Game Over to
  pass the time.
- **Is basically complete.** I'm married to my tradeoffs but
  still poke when fancy strikes. Maybe you can show me an
  angle I hadn't considered though? Would love to make it
  _even denser_ somehow!

<!--
I see you there, reading the raw markdown source. Yeah, TTC
are touchy about their mark, so ya gotta be real careful when
you say "Tetris." We all know this. It's better left unsaid.

But I do mean it earnestly as well: this thing doesn't try to
be a slick product anyone would want to play. It's my toybox.
Come, play with me.
-->

## Further Reading
<!--------------->

- [Design Tour][des]: Tutorial, background, reference.
- [Tinkering][tin]: Make your own dev environment.
- [Forth source][sss]: Damn dense, beware dragons.

[des]: Design.md
[inp]: Design.md#input
[sco]: Design.md#sound
[sou]: Design.md#score
[den]: Design.md#density
[tin]: Tinkering.md
[hfs]: Tinkering.md#getting-more-leverage
[sss]: sss.fs

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/

<!-- end of README.md -->
