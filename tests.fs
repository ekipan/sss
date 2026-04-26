\ \ included by sss.fs 'test' word.

require recent.fs \ helpful.
require sss.fs \ start w/ either file.
marker --tests-- \ reclaimed if ok.
: test  --tests-- test ; \ if failed.

\ set known state for tests.
decimal bg ss \ and keep current game.
: reset ( -) 123 seeded ; reset

depth constant d0 \ tests, sections:
: & ( nfn-f) rot = and ;
: ? ( f-) 0= abort" x" '.' emit ;
: .. ( u-) cr . depth d0 - . reset
  s" -- marker --" evaluate ;
marker -- \ reset each section.

7 .. \ >10+> 4* 40*

20 40 >10+> 40 = ?  30 = ? \ add over.
2 4* 8 = ?  2 40* 80 = ? \ fast mul.

6 .. \ profile erase rdrop split

: p 999 0 do loop ; ' p 6 profile
' p over > ?  '' p = ? \ get old xt.
1 prof p 0 prof \ blue screen flash.
create e $1234 , $5678 ,  e 1+ 2 erase
e @ $34 = ?  e 2+ @ $5600 = ? \ c64=le.
: r ( -a; not tco safe.) [ w stx, tsx,
  txa, w ldx, 1 ldy,# ] pushya ;
: r ( -aaaa) r r 0 >r r rdrop r [ ] ;
r rot = ?  > ? \ fast r> drop.
$1234 split $12 = $34 & ? \ bytes.

space \ kbpoll entropy th-c w! b@

$3738 $277 ! 2 $c6 c! \ buffer keys.
kbpoll '8' = ? kbpoll '7' = ?
kbpoll 0= ? \ empty.
entropy ? \ never zero.
\ both at the bottom left of well:
colormem $d800 $dbe8 within ?
colormem $d800 - tilemem $400 - = ?
0 th-c colormem = ?
1 0 th-c c! \ put white dot at origin.
$205 th-c colormem 75 - = ? \ 40 b/r.
: b ( pa-pp) w! b@ ; \ w is volatile.
11 here 22 , b 11 = ?  33 = ?

5 .. \ n: >p th-g

3 4 n: * 12 = ? \ parse and execute.
n: $10 #16 = ? \ $hex and #dec.
: n  n: 1 ;  n 2  1 = 2 & ?
2 >p 0 = ?  $33 >p $0301 = ? \ table.
7 th-g c@ 8 th-g c@ > ? \ per 8 lines.

space \ piece

\ 0-$1609 0-3 0-6 piece -> 4*$1609 2-8
0 0 0 piece \ btm-left flat i-piece.
3 = 1 & 0 & -1 & -2 & ? \ 3=cyan.
0 1 0 piece \ vertical:
3 = $300 & $200 & $100 & 0 & ?
$1305 2 1 piece \ top flipped j.
8 = $1404 & $1306 & $1305 & $1304 & ?
$1305 2 1 piece plot \ on screen.

4 .. \ var+ hold pinned? unpin

5 1 var+ v  6 = ?  v 5 = ? \ def/scan.
reset 1 well +! well @ 1 = ?
reset well @ 0 = ?
5 shape c! hold pinned? ? \ hold pins.
shape c@ 5 = ? \ depends on seed.
unpin hold shape c@ 5 = ?

\ todo th-w th-q

3 .. \ todo

2 .. \ enqueue

: q enqueue ;  1 q 2 q 3 q 4 q enter
curr 1 = ? 0 = ? $1305 = ? \ top j.
reset 7 roll reset 7 roll = ?

space \ hit? lock

: i ( p-pppps) 0 0 piece ; \ flat i.
0 i hit? ? \ past left wall.
2 i hit? 0= ? \ in bounds.
2 i lock \ store in well.
well dup 40 + paint \ show on screen.
5 i hit? ? \ 1 block overlap.
6 i hit? 0= ? \ to the right.
$102 i hit? 0= ? \ above.

1 .. \ tick

create t 2 ,  t tick 0= ?  t tick ?

--tests-- ll \ ok!
