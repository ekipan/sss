\ \ : p 1 ;  : q p ;  patch: p 2 ;
\ "q ." now types "2". the first p is
\ overwritten w/ a jmp to the second.

: patch:  $4c here ' tuck 1+ ! c! ] ;

\ warning: patching a word w/ <3 code
\ bytes will corrupt the dictionary!
