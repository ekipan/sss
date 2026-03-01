\ \ included by sss.fs 'test' word.

require recent.fs \ helpful.
require sss.fs \ start w/ either file.
marker --tests-- \ reclaimed if ok.
: test  --tests-- test ; \ if failed.

depth constant d0 \ tests, sections:
: ? ( f-) 0= abort" x" '.' emit ;
: .. ( u-) cr . depth d0 - .
  s" -- marker --" evaluate ;
marker -- \ reset each section.
decimal 123 seeded bg \ known state.

7 .. \ preamble.

\ test: >10+> 4* 40*
20 40 >10+> 40 = ?  30 = ? \ add over.
2 4* 8 = ?  2 40* 80 = ? \ fast mul.

6 .. \ tools and nonportable.

\ test: profile erase rdrop split
: p ;  ' p  0 profile  \ add code.
' p over > ?  '' p = ? \ get old xt.
create e $1234 , $5678 ,  e 1+ 2 erase
e @ $34 = ?  e 2+ @ $5600 = ? \ c64=le.
: r ( -a; not tco safe.) [ w stx, tsx,
  txa, w ldx, 1 ldy,# ] pushya ;
: r ( -aaaa) r r 0 >r r rdrop r [ ] ;
r rot = ?  > ? \ fast r> drop.
$1234 split $12 = ?  $34 = ? \ bytes.

space

\ test: kbpoll entropy th-c w! b@
$3738 $277 ! 2 $c6 c! \ buffer keys.
kbpoll '8' = ? kbpoll '7' = ?
kbpoll 0= ? \ empty.
entropy ? \ never zero.
0 th-c colormem = ?
1 0 th-c c! \ put white dot at origin.
$205 th-c colormem 75 - = ?
: b w! b@ ; \ w is very volatile.
11 here 22 , b 11 = ? 33 = ?

5 .. \ data, piece definition.

\ test: n: >p piece th-g
3 4 n: * 12 = ? \ parse and execute.
n: $10 #16 = ? \ $hex and #dec.
: n  n: 1 ;  n 2  1 = ?  2 = ?
2 >p 0 = ? \ table to blockspace.
$33 >p $0301 = ?

\ todo more tests! sections 5 thru 1.

space --tests-- \ all done.
