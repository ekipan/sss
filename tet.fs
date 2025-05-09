marker --tet-- \ for durexforth 4.

\ 0 . groundwork.

: h. ( u-) hex u. decimal ; \ devtools.
: redo ( -) --tet-- s" tet" included ;
create bd/  $d020 dup eor, sta, rts,
: pro ( enable? -- ) if
  $4d else $60 then bd/ c! ;
: profile ( color -- ) latest >xt over
  here latest name>string + ! ( cxc)
  lda,# bd/ jsr, jsr, lda,# bd/ jmp, ;
\ : pro drop ; : profile drop ;

: sync ( -) [ $d5 lda,#     \ hardware.
  $d012 cmp, -5 bne, ] ;  13 profile
: theme ( uu-) $d020 ! $286 c! ;
: entropy ( -u) $a1 @ dup 0= + ;
: kinit ( -) $b80 $28a ! 0 $c6 c! ;
: kb ( -c; w/ fast repeat hack.) key?
  if 1 $28b c! key else 0 then ;
: w! ( a-) [ lsb ldy,x w sty,    \ opt.
  msb ldy,x w 1+ sty, inx, 0 ldy,# ] ;
: p@+ ( p-pp) dup [ clc, w lda,(y) iny,
  lsb 1+ dup adc,x sta,x w lda,(y) iny,
  msb 1+ dup adc,x sta,x ] ;
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;

-1 value -1 3 value 3           \ math.
10 value #10 22 value #22 40 value #40
: 40- #40 - ; : >10+> swap #10 + swap ;
: 4*  2* 2* ; : 10*  2* dup 4* + ;
: 0* drop 0 ; : 40*  4* 10* ;
: erase ( au-) 0 fill ;       \ basics.
: else: ( -) postpone exit
  postpone then ; immediate
: if\ ( f-) if postpone \ then ;
: field ( au-a) over constant + ;
: n: ( -?) parse-name evaluate ;
: c: ( u-) 0 do n: c, loop ;
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p:  hex 8 0 do n: >p , loop decimal ;

1 . \ piece definition.

create colors 7 c: 3 8 6 4 5 2 7
create blocks \ compiled as $0y0x-2.
p: 00 01 02 03  02 12 22 32  \ iixi
p: 00 01 02 03  02 12 22 32  \
p:  03 11 12 13  01 02 12 22 \   jjj
p:  01 02 03 11  02 12 22 23 \    .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
p:  02 11 12 13  02 11 12 22 \   ttt
p:  01 02 03 12  02 12 13 22 \    x
p: 01 02 12 13  02 11 12 21  \  ss
p: 01 02 12 13  02 11 12 21  \ sx
p:  03 02 11 12  02 12 13 23 \   zz
p:  03 02 11 12  02 12 13 23 \    xz
p: 01 02 11 12  01 02 11 12  \ oo
p: 01 02 11 12  01 02 11 12  \ ox

\ given a piece (p)osition $yyxx,
\ (t)urn count 0-3, and (s)hape 0-6,
\ fetch 4 blocks and a (c)olor.
: piece ( pts-ppppc) dup colors + c@ >r
  4* + 4* 2* blocks + w! p@+ p@+ p@+
  p@+ drop r> ;  5 profile

2 . \ game state.

here 247  2dup 1 fill  allot
210 field well \ 10 cols 22 rows of:
10 field spill \  0 empty, 1 marked,
0 field top    \  2-8 block colors.
2 field seed  \ for random generator.
2 field %show \ n->0 line sweep timer.
2 field %grav \ n->0 fall timer.
2 field lines \ todo: gravity curve.
2 field qp    \ mod8 queue index:
8 field queue \ random shapes 0-6.
1 field kept  \ 0-6   hold, w/ pin bit.
2 field p1    \ $yyxx from bottom left.
1 field t1    \ 0-3   clockwise turns.
1 field s1    \ 0-6   shape ijltszo.
2 field p0    \ drawn previous frame,
2 field t0    \ to be erased.
' well - ?dup 0= if\ rvs . cr abort

3 . \ hold, tgmlike random queue.

\ avoid name clash with forth 'hold'.
: kept! ( s-) kept c! ;
: kept@ ( -s) kept c@ 7 and ;
: pinned? ( -f) kept c@ 8 and ;
: unpin ( -) kept@ kept! ;
: keep  kept@ s1 c@  8 or kept! s1 c! ;

: roll ( u-u) seed @ $7abd * $1b0f +
  dup seed ! um* nip ;  5 profile
: reroll ( s-s) drop 7 roll ;
: sz-ijlt ( -ss) 2 roll 4 +  4 roll ;

\ (f) reroll? if dupe, up to 4 times.
: th-q ( i-a) qp c@ + 7 and queue + ;
: qdup? ( sfi-sf) swap if drop 1 else:
  th-q c@ over = ;  15 profile
: qtry ( sf-sf) if reroll 0 0 qdup?
  1 qdup? 2 qdup? 3 qdup? else: 0 ;
: qnext ( -) 0 1 qtry qtry qtry
  if reroll then  1 qp +!  3 th-q c!
  0 th-q c@ s1 c! ;  7 profile
create ssz 5 c: 0 0 4 4 5 \ bias -s/z.
: qinit ( -) sz-ijlt  3 th-q c!  kept!
  ssz qp 5 move  qnext qnext qnext ;

4 . \ the well.

: th-y ( y-a) 10* well + ;
: th-p ( p-a) split 10* + well + ;
  4 profile

\ whiten, count, move down lines.
: full? ( a-f) 1 swap dup >10+> do
  i c@ 0= if 1- leave then loop ;
: m+ ( au-au) over full? if  1+ over
  #10 1 fill then  >10+> ;  12 profile
: mark ( y-u) th-y 0 m+ m+ m+ m+ nip ;
: s+ ( aa-aa) over c@ 1- if  2dup #10
  move #10 + then  >10+> ;  12 profile
: sweep ( -) well well begin s+ s+ over
  top = until  nip top over - erase ;

\ check, store into well.
: h? ( pf-f) swap dup  split #22 u<
  swap #10 u< and  if th-p c@ then or ;
: hit? ( ppppc-f) 0* h? h? h? h? 0<> ;
: l! ( pc-c) tuck swap th-p c! ;
: lock ( ppppc-) l! l! l! l! drop ;

5 . \ draw/update shared state.

1 constant &curr \ in(v)alidation
2 constant &next \ queue. set bits
4 constant &well \ to request
8 constant &kept \ redraw.
0 value inval
: &+ ( v-v) dup 1- or ;
: iv! ( v-) inval or to inval ;
: iv? ( v-f) inval and ;

\ the drawn piece is touched to erase
\ next frame. touch a new one to leave
\ the old one on screen.
: touch ( -) p1 p0 4 move ;
: drawn ( -pts) p0 @ t0 @ split ;
: curr  ( -pts) p1 @ t1 @ split ;
: curr-y ( -y) p1 1+ c@ ;

6 . \ game rules.

\ movement. bias kicks ccw>l cw>r.
$-100 constant down
: t1@+ ( t-t) t1 c@ + 3 and ;
: go? ( pt-f) over p1 @ +  over t1@+
  s1 c@  piece hit? if 2drop 0 else:
  t1@+ t1 c!  p1 +!  &curr iv! 1 ;
: tk ( pt-) go? if r> r> 2drop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  r> drop ;

\ (g)ameover if entry blocked.
: enter ( -) $1305 p1 ! 0 t1 c! ;
: trykeep ( -) pinned? if else:
  keep enter  &kept &curr or iv! ;
: fall ( -g) 12 %grav !  down 0 go?
  if 0 else:  kinit unpin
  curr piece lock  curr-y mark ?dup if
    lines +! 12 %show !
  &well else  &next &+ then  iv!
  qnext enter touch  curr piece hit? ;
: init ( u-) well ['] well over - erase
  seed !  qinit enter touch ;

: tick? ( a-f) -1 over +! c@ 0= ;
: update ( -g) %show c@ if  %show tick?
    if sweep &well &+ iv! then 0
  else:  %grav tick? if fall else:
  kb case  0 of endof
  's' of -1 0 go? drop endof
  'd' of fall exit endof
  'f' of 1 0 go? drop endof
  'j' of -1 turnkick endof
  'k' of 1 turnkick endof
  'l' of trykeep endof
  drop 1 exit endcase 0 ;

7 . \ drawing.

13 22 ( left bottom ) 40* + dup
$0400 + value screen
$d800 + value colormem

\ rvspace $a0, bg color squares. also
\ ignored by forth for easier testing.
: bg ( -) screen 38 + 21 0 do  dup 19
  $a0 fill 40- loop  2+ #10 $a0 fill ;

: th-cm ( p-a) split 40* - colormem + ;
: p! ( pc-c) tuck swap th-cm c! ;
: plot ( ppppc-) p! p! p! p! drop ;
: slot ( sp-) dup th-cm 2 - dup 40-
  4 erase 4 erase 0 rot piece plot ;
: w+ ( aa-aa) 2dup #10 move >10+> 40- ;
  12 profile

: draw ( -) &curr iv?  sync  if
    drawn piece 0* plot
  then  &well iv? if
    well colormem begin  w+ w+ w+
    over spill = until  2drop
  then  &curr iv? if
    curr piece plot  touch
  then  &kept iv? if
    kept@ $060d slot
  then  &next iv? if
    1 th-q c@ $100d slot
    2 th-q c@ $0d0d slot
    3 th-q c@ $0a0d slot
  then  0 to inval ;  6 profile
: dr ( -) -1 iv! draw ;

\ 8 . main loop words (-).

: pre  kinit 11 0 theme page bg dr ;
: r  pre begin update draw until ;
: new  0 pro entropy init r ;
: help  ." cmds: new r(esume)"
  ."   keys: sdf jkl q " ;
1234 init  ' help start !
