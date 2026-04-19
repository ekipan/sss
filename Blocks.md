This is a rationale for the [blocks table][blo] of [SSS].

Each piece exists in a 4x4 blockspace where
-2 <= x <= 1 and 0 <= y <= 3:

```
3 . . . .
2 . . . .
1 . . . .
0 . . x . <- origin
 -2-1 0 1
```

When `land`ing a piece, the phrase `row mark` starts the line
clear check at the origin row, so the origin itself must be
bounded by the piece's blocks or else it risks going out of
well bounds.

In both [ARS] and [SRS] the I piece biases to the right,
closer to the where players usually gap. The 3-wide pieces
JLTSZ, however, bias to the left, so to support both I'd have
to code exceptions. Instead I chose to uniformly bias right,
which is simpler, though it clashes with veteran player muscle
memory.

```
spawn in well:         rotate clockwise:

|    I0 . . . .      | I1 . .[] . I2 I3 repeat
|       . . . .      |    . .[] .
|       . . . .      |    . .[] .
| . . .[][]()[] . . .|    . .() .

|    J0 . . . .      | J1 . . . . J2 . . . . J3 . . . .
|       . . . .      |    . .[] .    . . . .    . .[][]
|       .[][][]      |    . .[] .    .[] . .    . .[] .
| . . . . . x[] . . .|    .[]() .    .[]()[]    . .() .

|    L0 . . . .      | L1 . . . . L2 . . . . T3 . . . .
|       . . . .      |    .[][] .    . . . .    . .[] .
|       .[][][]      |    . .[] .    . . .[]    . .[] .
| . . . .[] x . . . .|    . .() .    .[]()[]    . .()[]

|    T0 . . . .      | T1 . . . . T2 . . . . T3 . . . .
|       . . . .      |    . .[] .    . . . .    . .[] .
|       .[][][]      |    .[][] .    . .[] .    . .[][]
| . . . . .() . . . .|    . .() .    .[]()[]    . .() .

|    S0 . . . .      | S1 . . . . S2 S3 repeat
|       . . . .      |    .[] . .
|       . .[][]      |    .[][] .
| . . . .[]() . . . .|    . .() .

|    Z0 . . . .      | Z1 . . . . Z2 Z3 repeat
|       . . . .      |    . . .[]
|       .[][] .      |    . .[][]
| . . . . .()[] . . .|    . .() .

|    O0 . . . .      | O1 O2 O3 repeat
|       . . . .      |
|       .[][] .      |
| . . . .[]() . . . .|
```

The spawn orientation 0 is pointy-end down and J2 L2 T2 are
downshifted to lie flat, consistent with [ARS] and opposed to
[SRS]. Unlike both, I2 _also_ rests on row 0, obviating much
of the need for floorkicking, which is unimplemented.

Rows in the well count up from the bottom 0 <= y <= 22, and
pieces spawn with center at row 19.

Back to the design run-down:
[continue][blo] or [from the top][des].

[sss]: README.md
[des]: Design.md
[blo]: Design.md#the-blocks-table
[ars]: https://tetris.wiki/Arika_Rotation_System
[srs]: https://tetris.wiki/Super_Rotation_System
