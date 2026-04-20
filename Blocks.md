This is a rationale for the [blocks table][blo] of [SSS].
The block table source (x, y) ranges (0..3, 0..3):

```
3   . . . .
2   . . . .
1   . . . .
0   . . . .

    0 1 2 3
```

In [ARS] the I piece biases to the right, closer to the where
players usually [gap]. In both ARS and [SRS] the 3-wide pieces
JLTSZ, however, bias to the left, so to support both I'd have
to code exceptions. Instead I chose to uniformly bias right,
which is simpler, though it clashes with veteran player muscle
memory.

```
spawn in well:         rotate clockwise:

|    I0 . . . .      | I1 . .[] . I2 I3 repeat
|       . . . .      |    . .[] .
|       . . . .      |    . .[] .
| . . .[][][][] . . .|    . .[] .

|    J0 . . . .      | J1 . . . . J2 . . . . J3 . . . .
|       . . . .      |    . .[] .    . . . .    . .[][]
|       .[][][]      |    . .[] .    .[] . .    . .[] .
| . . . . . .[] . . .|    .[][] .    .[][][]    . .[] .

|    L0 . . . .      | L1 . . . . L2 . . . . T3 . . . .
|       . . . .      |    .[][] .    . . . .    . .[] .
|       .[][][]      |    . .[] .    . . .[]    . .[] .
| . . . .[] . . . . .|    . .[] .    .[][][]    . .[][]

|    T0 . . . .      | T1 . . . . T2 . . . . T3 . . . .
|       . . . .      |    . .[] .    . . . .    . .[] .
|       .[][][]      |    .[][] .    . .[] .    . .[][]
| . . . . .[] . . . .|    . .[] .    .[][][]    . .[] .

|    S0 . . . .      | S1 . . . . S2 S3 repeat
|       . . . .      |    .[] . .
|       . .[][]      |    .[][] .
| . . . .[][] . . . .|    . .[] .

|    Z0 . . . .      | Z1 . . . . Z2 Z3 repeat
|       . . . .      |    . . .[]
|       .[][] .      |    . .[][]
| . . . . .[][] . . .|    . .[] .

|    O0 . . . .      | O1 O2 O3 repeat
|       . . . .      |
|       .[][] .      |
| . . . .[][] . . . .|
```

The spawn orientation 0 is pointy-end down and J2 L2 T2 are
downshifted to lie flat, consistent with [ARS] and opposed to
[SRS]. Unlike both, I0 I2 _also_ rest on row 0, obviating much
of the need for [floorkicking][flo], which is unimplemented.

Back to the design run-down:
[continue][blo] or [from the top][des].

[sss]: README.md
[des]: Design.md
[dat]: Design.md#data-shorthands-c-b
[blo]: Design.md#the-blocks-table
[ars]: https://tetris.wiki/Arika_Rotation_System
[flo]: https://tetris.wiki/Floor_kick
[gap]: https://tetris.wiki/Stacking_for_Tetrises
[srs]: https://tetris.wiki/Super_Rotation_System

<!-- end of Blocks.md -->

