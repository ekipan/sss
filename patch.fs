\ \ : p 1 ;  : q p ;  patch: p 2 ;
\ "q ." now types "2". the first p is
\ overwritten w/ a jmp to the second.

: patch:  here ' $4c over c! 1+ ! ] ;
\ warning: needs >=3 code bytes!
\  : bad drop ;   \ inx, rts, \ 2
\  : victim 1 ;   \ pushone jmp, \ 3
\  patch: bad + ; \ breaks victim!
