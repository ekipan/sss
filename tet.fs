marker --tet-- \ tested on durexfth v4.

: else:  postpone exit        \ macros.
  postpone then ; immediate
: dup>r  postpone >r dex, ; immediate
: .h ( u-) hex u. decimal ; \ devtools.
: make ( -) --tet-- s" tet" included ;
: r/ [ 2 lda,# $d020 dup eor, sta, ] ;
: g/ [ 5 lda,# $d020 dup eor, sta, ] ;
: b/ [ 6 lda,# $d020 dup eor, sta, ] ;
: perf ( f-) if $a9 else $60 then dup
  dup ['] b/ c! ['] r/ c! ['] g/ c! ;

: erase ( au-) 0 fill ;       \ basics.
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;
: w! ( n-) [ lsb lda,x msb ldy,x inx,
  w sta, w 1+ sty, txa, ] ;
-1 value -1 3 value 3       \ fastmath.
10 value #10 23 value #23 40 value #40
: 10+ #10 + ; : >10+> swap 10+ swap ;
: 40- #40 - ; : 10* 2* dup 2* 2* + ;
: 0* drop 0 ; : 40* 2* 2* 10* ;
: kb ( -c) key? if          \ keyboard.
  [ 1 lda,# $28b sta, ] key else: 0 ;
: kbrep ( -) [ $80 lda,# $28a sta, ] ;
: kbflush ( -) [ 16 lda,# $28b sta,
  0 lda,# $c6 sta, ] ;

1 . \ piece definition.

: n: ( -?) parse-name evaluate ;
: c: ( u-) 0 do n: c, loop ;
: p: ( -) hex 8 0 do n: dup 4 lshift
  or $f0f and 1- 1- , loop decimal ;

create patterns \ compiled as $0y0x-2.
p: 00 01 02 03  02 12 22 32  \ oiii
p: 00 01 02 03  02 12 22 32  \
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \   .  j
p: 01 11 12 13  02 12 22 21  \  lll
p: 01 02 03 13  03 02 12 22  \ .l
p:  02 11 12 13  02 11 12 22 \    ttt
p:  01 02 03 12  02 12 13 22 \   . t
p: 01 02 12 13  02 11 12 21  \   ss
p: 01 02 12 13  02 11 12 21  \ .ss
p:  03 02 11 12  02 12 13 23 \    zz
p:  03 02 11 12  02 12 13 23 \   . zz
p: 01 02 11 12  01 02 11 12  \  oo
p: 01 02 11 12  01 02 11 12  \ .oo
\ tgmlike except horiz i lays on row 0.

create ttc 7 c: 3 8 6 4 5 2 7
create tgm 7 c: 2 8 6 3 4 5 7
ttc value colors

\ compute 4 (b)lock coords $yyxx and a
\ (c)olor for a piece given by origin,
\ (t)urn count 0-3, and (s)hape 0-6.
: b@+ ( ba-bba) dup>r @ over + swap
  r> 2+ ;
: piece ( bts-bbbbc) g/ dup>r 2* 2* +
  2* 2* 2* patterns + b@+ b@+ b@+ b@+
  2drop r> colors + c@ g/ ;

2 . \ game state.

: field ( au-a) over constant + ;
: if\ ( f-) if postpone \ then ;

here 256 2dup erase allot
210 field well  \ 10 cols 23 rows of:
20 field vistop \  0 empty, 1 marked,
0 field welltop \  2-8 block colors.
2 field %show  \ n->0 line sweep timer.
2 field %grav  \ n->0 fall timer.
2 field lines  \ todo: gravity curve.
2 field seed   \ cannot be zero!
1 field kept   \ 0-6  hold, w/ pin bit.
2 field qp     \ mod8 queue index:
8 field queue  \ random shapes 0-6.
2 field b1     \ $yyxx block position.
1 field t1     \ 0-3   clkwise turns.
queue value s1 \ cached queue addr.
2 field b0     \ $yyxx drawn previous
1 field t0     \ 0-3   frame, to be
1 field s0     \ 0-6   erased.
' well - ?dup 0= if\ rvs . cr abort

3 . \ shapes queue.

\ usually called a 'hold piece' but
\ i don't want to clash w/ 'hold'.
: kept! ( s-) kept c! ;
: kept@ ( -s) kept c@ 7 and ;
: pinned? ( -f) kept c@ 8 and ;
: unpin ( -) kept@ kept! ;
: pin ( -) kept@ 8 or kept! ;
: exchg ( -) kept@ s1 c@ kept! s1 c! ;

: roll ( u-u; xorshift-798.) [ seed 1+
  lda, ror,a seed lda, ror,a seed 1+
  dup eor, sta, ror,a seed dup eor,
  sta, seed 1+ dup eor, sta, tay, seed
  lda, ] pushya r/ um* r/ nip ;
: reroll ( s-s) drop 7 roll ;

\ tgm rng: reroll dupes up to 4 times.
: th-q  r/ qp c@ + 7 and queue + r/ ;
: q! ( s-) 3 th-q c! ;
: q+ ( u-) qp +!  0 th-q to s1 ;
: qdup ( sfi-sf) swap if drop 1 else:
  th-q c@ over = ;
: qtry ( sf-sf) if 1 else: reroll 0
  -1 qdup 0 qdup 1 qdup 2 qdup 0= ;
: qnext ( -) 1 q+  0 0 qtry qtry qtry
  0= if reroll then q! ;

\ open w/ ijlt, bias against sz.
create ssz 5 c: 0 0 4 4 5
: qflush ( -) ssz qp 5 move  4 roll q!
  qnext qnext qnext  2 roll 4 + kept! ;

4 . \ the well.

\ move unmarked lines down, erase top.
: drain ( a-) welltop over - erase ;
: 10move ( aa-) r/ #10 move r/ ;
: sweep ( -) well well begin  over c@
  1- if 2dup 10move 10+ then >10+> over
  welltop < 0= until  drain drop ;

\ whiten filled lines, give count (u).
: line ( au-au) over 1- w! [ 10 ldy,#
  w lda,(y) 1 bne, rts, dey, -8 bne, ]
  over #10 1 fill  1+ ;
: mark ( au-u) over + >r 0 begin
  r/ line r/ >10+>  over r@ < 0= until
  nip r> drop ;
: th-r ( y-a) 10* well + ;

\ check well, store into well.
\ reuse oob indices as true flag.
: th-b ( b-a) g/ split 10* + well +
  g/ ;
: h? ( bf-f) swap dup split #23 u<
  swap #10 u< and if th-b c@ then or ;
: l! ( bc-c) tuck swap th-b c! ;
: hit? ( bbbbc-f) 0* h? h? h? h? 0<> ;
: lock ( bbbbc-) l! l! l! l! drop ;

\ 4.5.  draw/update shared state.

1 constant &curr \ invalidation
2 constant &kept \ queue. set bits
4 constant &next \ to request
8 constant &well \ redraw.
0 value inval
: iv? ( u-f) inval and ;
: iv! ( u-) inval or to inval ;
: etc ( u-u) dup 1- or ;

\ 'draw' touches the drawn piece to
\ erase next frame. touch a new one
\ to leave the old one on screen.
: touch ( bts-) s0 c! t0 c! b0 ! ;
: drawn ( -bts) b0 @ t0 c@ s0 c@ ;
: curr  ( -bts) b1 @ t1 c@ s1 c@ ;
: curr-4r ( -au) b1 1+ c@ th-r #40 ;

5 . \ player update.

\ move piece. kick into turn direction.
: t1@+ ( t-t) t1 c@ + 3 and ;
: go? ( fbt-f) rot 0= if 2drop 0 else:
  over b1 @ +  over t1@+  s1 c@
  piece hit? if 2drop 1 else:
  t1@+ t1 c!  b1 +!  0 &curr iv! ;
$-100 constant down
: shift? ( b-f) 1 swap 0 go? ;
: turnkick ( t-) dup>r 0 i go? i i go?
  0 i - i go? down i go? down i + i go?
  down i - r> go? drop ;

\ (g)ameover if entry blocked.
: enter ( u-) iv! $1305 b1 ! 0 t1 c! ;
: keep ( -) pinned? if else:
  exchg pin &kept etc enter ;
: fall? ( -g) 12 %grav !  down shift?
  0= if 0 else:  kbflush unpin
  curr piece lock  curr-4r mark ?dup if
  lines +! 11 %show ! 3 %grav ! &well
  else &next etc then  enter qnext
  curr touch  curr piece hit? ;
: init ( u-) seed !  well seed well -
  erase  &well etc enter qflush
  curr touch  kbrep ;

: tick? ( a-f) -1 over +! c@ 0= ;
: update ( -g) %show c@ if  %show
    tick? if sweep &well etc iv! then
  0 else:  %grav tick? if fall? else:
  kb $7f and case  0 of endof
  'j' of -1 turnkick endof
  'k' of 1 turnkick endof
  's' of -1 shift? drop endof
  'f' of 1 shift? drop endof
  'd' of fall? exit endof
  'l' of keep endof
  drop 1 exit endcase 0 ;

6 . \ drawing.

$077d value screen   \ bottom left
$db7d value colormem \ of well.

: th-cm ( b-a) b/ split 40* - colormem
  + b/ ;
: p! ( bc-c) tuck swap th-cm c! ;
: plot ( bbbbc-) p! p! p! p! drop ;
: slot ( sb-) dup th-cm 2 - dup 40-
  4 erase 4 erase 0 rot piece plot ;

: sync ( -) b/ [ $c5 lda,# $d012 cmp,
  -5 bne, ] b/ ;
: draw ( -) &curr iv?  sync  if
    drawn piece 0* plot
  then  &well iv? if
    well colormem begin
    2dup 10move >10+> 40-
    over vistop < 0= until  2drop
  then  &curr iv? if
    curr piece plot  curr touch
  then  &kept iv? if
    kept@ $070d slot
  then  &next iv? if
    1 th-q c@ $110d slot
    2 th-q c@ $0e0d slot
    3 th-q c@ $0b0d slot
  then  0 to inval ;

: help ( -) ." cmds: new r(esume)"
  ."    keys: sdf jkl q" cr ;
: hue ( fg $bgbd ) $d020 ! $286 c! ;
: rblank ( au-) $a0 fill ;
: canvas ( -) screen 38 + 21 0 do dup
  19 rblank 40- loop 2+ #10 rblank ;
: dw  0 q+ -1 iv! draw ;
: da  11 0 hue page canvas help dw ;

\ main loop.

: r  da begin update draw until ;
: fresh ( -u) $a1 @ 1 or ;
: new  0 perf fresh init r ;
3 init cr help

