\ \ ex: 7 5 bench * ( measure cycles. )
code now ( -j) sei, $a1 ldy, $a2 lda,
  cli, ' pushya jmp,

code ti  here $a ldx,# $b jmp,
1+ dup dup 2+ ( 'a 'a 'b )
: ti ( x-u) literal ! [ stx, ] now >r
  463 0 do ti ti ti ti loop
  [ ldx, ] now r> - 9 * ;
\ 2*2*3*3*463=16668 ~= cycles/jiffy.

here rts, \ subtract nop overhead:
: ti ( x-u) ti literal ti - ;
: bench ( '-) ' ti u. ;
