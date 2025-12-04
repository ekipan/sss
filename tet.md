[tet.fs][src] is extremely dense, as its intended audience is
just myself. This README tries to be a guided tour for less
longbearded folk. Code excerpts might drift out-of-date, fair
warning.

## Trying It Out

The [durexForth][dur] system is available as either a C64
cartridge or disk image. Load durexForth then either paste
[the program][src] directly, or get it on a disk file somehow
and `include tet.fs`. durexForth comes with a vi clone if you
want to make a file that way: `v tet.fs`.

After compiling try `1 prof 123 init r` and framestep to see
gamestate update time in gray and draw time in blue.

I use [VICE's][vic] host filesystem feature to develop. It's
not officially supported by durexForth so it has a giant pile
of caveats but it's convenient enough that I use it anyway.
Caveats include needing ALL CAPS, needing CR line endings, and
not working in durexForth v5 for reasons I don't understand
yet, so I use v4.

[src]: #end-of-readme
[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/

## Tetris? Commodore 64? Forth?

Everyone knows the falling blocks game Tetris. If you somehow
don't, you could read [Tetris on Wikipedia][tow], but you
should definitely just **play Tetris** instead. [tetr.io][tio]
is a great way to play _right now._

[tow]: https://en.wikipedia.org/wiki/Tetris
[tio]: https://tetr.io/

**Design context:** I've played _far_ more The Tetris Company
(TTC) Tetris than Tetris The Grandmaster (TGM), but I have a
strong admiration for the latter, so tet.fs is a mix of both
of these specifications plus my own flair.

The Commodore 64 is the legendary and beloved computer from
the 1980s, which I was personally drawn to _only after_
stumbling on [durexForth][dur], a modern [ANS Forth][ans]
written for it.

Forth is an old and grumpy programming language that I adore.
[Forth on Wikipedia][fow] and the beloved
[Starting Forth][sta] are great places to start. Herein I say
"word" to mean a subroutine written in Forth.

[ans]: https://forth-standard.org/
[fow]: https://en.wikipedia.org/wiki/Forth_(programming_language)
[sta]: https://www.forth.com/starting-forth/

I use compact stack comments to fit the cramped C64 screen:

- `erase ( au-)` address, unsigned count -> (no result).
- `piece ( pts-ppppc)` piece position, turn count, shape index
  -> 4 block positions, color code.
- `hit? ( ppppc-f)` 4 positions, (ignored) color code
  -> boolean flag.
- `split ( $yyxx -- $xx $yy )` closer to conventional ANS
  style when I think the clarity is needed.

## Diving In

The heart of this program is the `piece` word that computes
block positions from a piece description. Here I ask for a
piece centered on entry position hex $1305 (row 19 column 5),
rotated twice, shaped like a J (shape index 1):

```forth
hex 1305 2 1 piece .s
1304 1305 1306 1404 8 ok
```

### Coordinates

Packed hex `$yyxx` coordinates exist in three spaces:

- **Blockspace:** `0 <= y <= 3, -2 <= x <= 1.`
  `$0000` = piece center for blocks and line fill check.
- **Wellspace `th-w`:** `0 <= y <= 22, 0 <= x <= 9`
  `$0000` = bottom left of playfield.
- **Screenspace `th-c`:** `0 <= y <= 20, 0 <= x <= 15.`
  `$0000` = screen row 22 col 13, near bottom left of canvas.

The well extends two rows above screen (21 and 22) for new
pieces to rotate into.

Screen origin is chosen to roughly center the canvas: the well
in columns 0-9, hold and next queue in columns 12-15, plus
extra borders below and to the sides.

The orange value `8` from earlier can be [`lock`ed][loc] into
the 4 well positions if not `hit?`-detected, or `plot`ted on
screen. These use memory indexing `th` words: `0 th-w` gives
the address of the **(0,0)th space in the well.**

```forth
0 th-w h. well h.
ca00 ca00 ok
$0405 th-w h. #45 + well + h.
ca2d ca2d ok
$0405 th-c h. -4 40* 5 + colormem + h.
dae2 dae2 ok
0 th-q h. 3 th-q h. ( queue head/tail )
caee caed ok
```

[pre]: #compile-blocks-p
[loc]: https://tetris.wiki/Glossary#L

We'll start by defining data compilation shorthands:

### Data words `c: p:`

```forth
: n: ( *'-*) parse-name evaluate ;
: c: ( u'-) 0 do n: c, loop ;
create colors 7 c: 3 8 6 4 5 2 7
```

`n: (*'-*)` asks the interpreter to parse `'` and interpret a
word. Since it could do literally anything I notate its effect
with `*`s but it's intended to parse number literals for
compiling data.

`c: (u'-)` loops `u` times, calling `n:` to parse a value and
`c,` to compile a character (i.e. byte) to memory.

`colors (-a)`, a `create`d word, pushes the `a`ddress after
itself, where I've compiled 7 bytes. C64 color codes for 7
Tetris pieces, in my preferred TTC color scheme (I = 3 cyan
etc). Without the shorthand I could have just written this as:
`create colors 3 c, 8 c, 6 c, 4 c, 5 c, 2 c, 7 c,`

```forth
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p: ( '-) hex 8 0 do n: >p , loop decimal ;
```

`>p (c-p)` does precomputation: expanding an 8-bit `c`haracter
hex `$yx` into 16-bit `$0y0x`, then subtracting center source
`02` into compiled blockspace `p`osition `$0000`, which may or
may not contain a block. Negative x borrows from the y coord
but `hit?` bounds checks before you can corrupt memory outside
of well and screenspace.

`p: ('-)` loops 8 times, parsing hex literals `n:`, expanding
them `>p`, then compiling them `,`.

### The `blocks` Table

```forth
create blocks \ center (c/.) at yx=02:
p: 00 01 02 03  02 12 22 32  \ iici
p: 00 01 02 03  02 12 22 32
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \     .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
( 4 shapes omitted. )
: p@ ( pa-ppa) dup >r @ over + swap r> 2+ ;
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + p@ p@ p@ p@ 2drop r>
  colors + c@ ;
```

`blocks (-a)` gives the `a`ddress of the table. Again I could
have written this without shorthand as below but the goal was
for the data in the source to be compact and easier to read.

```forth
create blocks \ compiled result values:
-2 , -1 , 0 , 1 , 0 , $100 , $200 , $300 ,
( etc etc )
```

The ASCII-art comments depict the first orientation of each
shape, [TGM-style pointy-end-down][ars] (please click, there
are illustrations!), though they all rest on row 0 and all are
centered on column 2 to simplify code instead of TGM's column
1 or 2.

[ars]: https://tetris.wiki/Arika_Rotation_System

For speed sake the table scanning word pair `w! (a-) p@ (p-pp)`
are written in assembly but for pedagogy sake I present here an
older combined Forth definition of `p@ (pa-ppa)`.

`p@ (pa-ppa)` takes a piece center position `p1` `$yyxx`, an
address in the blocks table `a1`, and fetches `@` and adds `+`
one cell of the table giving computed block position `p2`,
keeping the piece center `p3=p1`, and moving to the next table
address `a2=a1+2` ready to fetch the next block. The common
idiom `>r phrase r>` saves a value to the return stack,
allowing you to apply a `phrase` to the values underneath it.

`piece (pts-ppppc)` takes a center `p`osition `$yyxx`, `t`urn
count `0-3`, and `s`hape index `0-6`, combines `t` and `s` to
index into the blocks table, calls `p@` to scan out 4
`p`ositions, and then a `c`olor.

Besides the assembly `w! p@`, the rest of the program is Forth
and is just fast enough for [mostly full 50fps][per] during
normal play.

[per]: #performance

## Touring the Rest

In code order, so difficulty varies wildly.

### `>10+>`

```forth
: >10+> swap #10 + swap ;
```

The arrows are meant to evoke `>r 10 + r>`, but `swap` is
faster.

### `redo`

Is a deeply magical development convenience:

```forth
marker --tet--
: redo ( -) --tet-- s" tet.fs"
  included ( must tco! safe w/ df. ) ;
```

Consider what happens when you type `redo` at the interpreter.
It executes the marker `--tet--`, deleting the program,
including `redo` itself. The processor doesn't care, it
continues executing the code, now in free memory, pushing the
`( addr len )` of the file name and then calling `included`,
which recompiles the source.

In most Forths, execution would then try to return from
`included` back to `redo` code that might have moved, crashing
the system in a fireball. DurexForth, however, optimizes the
tail-call into a jump, so `included` returns directly to the
interpreter safely. The game variables outside the dictionary
then let you immediately resume a game in progress with the
new code.

### `profile`

```forth
create bx  $d020 eor, $d020 sta, rts,
: profile ( color -- ) here >r  dup
  lda,# bx jsr, latest >xt jsr, lda,#
  bx jmp,  r> latest name>string + ! ;
: prof ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
```

The code at `bx` exclusive-ors the C64 border color register
at `$d020`. `profile` adjusts the latest word to point to new
code: `lda #color  jsr bx  jsr oldcode  lda #color  jmp bx`,
instrumenting the word with border-flipping behavior for
visual time inspection while framestepping. The phrase
`name>string +` addresses the code field stored after the
name.

`prof` patches the first instruction at `bx` into either an
`eor` or `rts`, enabling or disabling it.

### `sync`

```forth
: sync ( -) [ 213 lda,# $d012 cmp, \ hw
  -5 bne, ] ;  6 profile
```

Tradeoffs: (1) as above, hard code raster line 213: correct by
construction. (2) parameterize on 8-bit input: incorrect for
lines 0-54 and 256-311. (3) parameterize on 9-bit input: more
argument and loop code.

### `bg`

```forth
: bg ( ... ) $a0 ( rvspace ) ( ... ) ;
```

The reverse-video spaces make pleasant squares and also are
ignored by the interpreter to make testing and experimenting
easier.

### `init`

```forth
: entropy ( -u) $a1 @ dup 0= + ;
: init ( u-) well size erase  seed !
  4 enqueue 5 enqueue 4 enqueue 4 roll
  enqueue  5 held!  enter  99 sig c! ;
: init ( u-) init qnext qnext qnext ;
: r ( -) 99 sig c@ <> if ;then kbinit
  bg #all d! begin draw step until ;
: new ( -) entropy init r ;
```

Starting a `new` game fetches part of the [jiffy counter][jif]
to seed the game state then enters the main loop, which is
named `r` for easy typing by the player.

The initial queue mimics [TGM randomizer][ran] behavior. First
the queue is [S, Z, S, random I/J/L/T], then after flushing
(`qnext` three times), the player starts with I/J/L/T, and the
next 3 pieces are less likely to be S or Z (4 or 5).

Most Tetris documentation lists the shapes alphabetically:
IJLOSTZ, I put IJLT first to simplify this init.

[jif]: https://www.c64-wiki.com/wiki/160-162
[ran]: https://tetris.wiki/TGM_randomizer

### `qnext`

```forth
\ roll (u-u) 0 <= u2 < u1.
: q? ( si-s/si-) th-q c@ over =
  if drop rdrop then ;
: qn ( -) 7 roll 0 q? 1 q? 2 q? 3 q?
  enqueue r> rdrop >r ;  12 profile
: qnext ( -) qn qn qn 7 roll enqueue ;
```

`qnext` itself reflects the complexity of the TGM algorithm. It
uses `rdrop` to affect control: either breaking out or rolling
again, though `qn` has to take care to dodge the profiling
instrument. Here's a more conventional version more programmers
will understand easier:

```forth
: reroll ( s-s) drop 7 roll ;
: q? ( sfi-sf) swap if drop 1 ;then
  th-q c@ over = ;
: qn ( sf-sf) if reroll 0
  0 q? 1 q? 2 q? 3 q? ;then 0 ;
: qnext ( -) 0 1 qn qn qn
  if reroll then enqueue ;
```

This isn't critical path so the codesize and cycle savings
aren't important but I choose to spend the extra `rdrop`
cognitive load just for its aesthetic, which I'm fond of.
`qnext` in both versions enqueues only once per call.

### `go` and `turnkick`

```forth
: t@+ ( t-t) turns c@ + 3 and ;
: curr+ ( pt-pts) swap pos @ +
  swap t@+  shape c@ ;
: curr+! ( pt-) t@+ turns c!  pos +! ;
: go ( pt-f) 2dup curr+ piece hit?
  if 2drop 1 ;then  curr+! #go d! 0 ;
$-100 constant down
: tk ( pt-) go 0= if rdrop rdrop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  rdrop ;
```

`go` adds `p`osition and `t`urn offsets to the current player
variables `pos` and `turns`, hit-checks the new hypothetical
piece position, and updates those variables if the piece can
move there. `turnkick` calls it up to six times to implement
[wallkicking][wal] upon rotation.

There are no [floor kicks][flo]. TGM3 allows limited I and T
floorkicking, but instead I have the horizontal rotations of
the I piece rest on row 0 so the player has less need, though
it's strictly easier than TGM in that sense.

[wal]: https://tetris.wiki/Wall_kick
[flo]: https://tetris.wiki/Floor_kick

## Performance

The PAL C64 has a ~19,700 cycle budget per 50Hz frame.
Estimated cycle costs, eyeballing `1 prof` color bands vs
8-line screen rows, ~500 cycles per:

- 750 = `piece`.
- 2500-3000 = `hit?`.
- 3250-3750 = `piece hit?` total.
- 70-1400 = KERNAL interrupt. Rolls through the frame,
  stepping on `sync` and dropping 1 frame per 4 or 5.
- 2500 = idle `draw step` (heaps of margin).
- 19000 = hold `j` to rotate every frame (very tight).
- maybe 3-4 entire frames = `land`. it's a lot of work, but I
  don't want to spend lots of complexity spreading it across
  frames. The stutter is acceptable between pieces IMO.

## What's Missing?

### Score

`lines` count progresses through the gravity frames table but
no scoring beyond that. The performance cost of computing
digits, the complexity cost of BCD, the digits on-screen
interfering with interpreter experimentation, just thinking
about it doesn't spark joy in me.

### Ghost Piece

I lament the missing [ghost piece][gho] and the [instant
drop][dro] that it enables. It's not possible to fit in the
frame budget as currently written. Not enough cycles to check
up to twenty `piece hit?`s.

[gho]: https://tetris.wiki/Ghost_piece
[dro]: https://tetris.wiki/Drop

Some approaches: caching, precalcing, incremental, probably
more. Some rough mental math and I think it might cost 10+ish
frames to compute all ghosts at entry time. I've seen NES
Tetrises with the feature but the complexity cost easily
outspends my joy budget.

<a name="end-of-readme"></a>
