bits 16

org 0x0

SECTION .text
_main:

	mov ax, cs
	mov ds, ax
	mov ax, 0xB800
	mov es, ax
	mov bx, 0

	mov ah, 0x0
	mov al, 0x1
	int 0x10 ; set video to text mode
	; main task is always active
	mov byte [task_status], 1

	mov dx, _print_blank_screen
	call _spawn_new_task

_update_game_state:
	loop_forever_main:
	
	call _updateplayer1
	call _updateplayer2
	call _updateball
	
	call _yield
	jmp loop_forever_main
	
	
	_updateball:
	push ax
	mov ax, word[ball_Y]
	cmp ax, 1
	jng _jmp_lesser
	jne _continue_test
	_jmp_lesser:
	jmp _limit_up
	
	
	
	_continue_test:
	cmp ax, 24
	jg _jmp_g
	jne _update_y
	_jmp_g:
	jmp _limit_down
	
	_update_y:
	mov ax, word [ball_direction]
	add word [ball_Y], ax
	pop ax
	ret
	
	_limit_down:
	mov word [ball_direction], -1
	jmp _update_y
	
	_limit_up:
	mov word [ball_direction], 1
	jmp _update_y
	
	_updateplayer1:
		push ax
		mov ax, [player1_X]
		cmp ax, 25
		jg _jmp3
		jne _continue_testx
		_jmp3:
		jmp _upper_limtX
		
		_continue_testx:
		cmp ax, 0
		jng _jmp4
		jne _updateX
		_jmp4:
		jmp _lower_limtX
		
		_updateX:
		mov ax, word [player1_direction]
		add [player1_X], ax
		pop ax
		ret

		_upper_limtX:
			mov word [player1_direction], -1
			jmp _updateX
			
		_lower_limtX:
			mov word [player1_direction], 1
			jmp _updateX
			
		
		
		
	_updateplayer2:
	push ax
	mov ax, word [player2_X]
	cmp ax, 25
	
	jg _jmp1
	jne _continue_test2x
	_jmp1:
	jmp _upper_limt2X
	
	_continue_test2x:
	cmp ax, 0
	jng _jmp2
	jne _update2X
	_jmp2:
	jmp _lower_limt2X
	
	_update2X:
	mov ax, word [player2_direction]
	add [player2_X], ax
	pop ax
	ret

	_upper_limt2X:
		mov word [player2_direction], -1 
		jmp _update2X
		
	_lower_limt2X:
		mov word [player2_direction], 1
		jmp _update2X
	
	
	; does not terminate or return

; dx should contain the address of the function to run
 _spawn_new_task:
	 mov bx, stack_pointers
	 add bx, [current_task]
	 ; save current stack pointer
	 add bx, [current_task] ; add twice because we have two bytes
	 mov [bx], sp
	 ; switch to new stack
	 mov cx, 0
	 mov cl, [current_task]
	 inc cl
 sp_loop_for_available_stack:
	 cmp cl, byte [current_task]
	 jne sp_check_for_overflow
	 jmp sp_no_available_stack
 sp_check_for_overflow:
	 cmp cl, 1
	 jg sp_reset
	 jmp sp_check_if_available
 sp_reset:
	 mov cl, 0
	 jmp sp_loop_for_available_stack
 sp_check_if_available:
	 mov bx, task_status
	 add bx, cx
	 cmp byte [bx], 0
	 je sp_is_available
	 inc cx
	 jmp sp_loop_for_available_stack
 sp_is_available:
	 mov bx, task_status
	 add bx, cx
	 mov byte [bx], 1
	 ; push a fake return address
	 mov bx, stack_pointers
	 add bx, cx
	 add bx, cx
	 mov sp, [bx]
	 push dx
	 ; push registers
	 pusha
	 ; push flags
	 pushf
	 ; update stack pointer for task
	 mov bx, stack_pointers
	 add bx, cx
	 add bx, cx ; add twice because we have two bytes
	 mov [bx], sp
	 ; restore to original stack
 sp_no_available_stack:
	 mov bx, stack_pointers
	 add bx, [current_task]
	 add bx, [current_task] ; add twice because we have two bytes
	 mov sp, [bx]
	 ret

_yield:
	; push registers
	pusha
	; push flags
	pushf
	; save current stack pointer
	mov bx, stack_pointers
	add bx, [current_task]
	add bx, [current_task] ; add twice because we have two bytes
	mov [bx], sp
	; switch to new stack
	mov cx, 0
	mov cl, [current_task]
	inc cl
	
y_check_for_overflow:
	cmp cl, 1
	jg y_reset
	jmp y_task_available
y_reset:
	mov cl, 0
	jmp y_check_for_overflow
y_task_available:
	mov bx, cx
	mov [current_task], bx
	; update stack pointer
	mov bx, stack_pointers
	add bx, [current_task]
	add bx, [current_task] ; add twice because we have two bytes
	mov sp, [bx]
	; pop flags
	popf
	; pop registers
	popa
	ret

; ====================== Print Logic ====================== ;
; Reset sets screen to blank green
_print_blank_screen:    
    mov ax, 0
    mov bx, 0
    mov cx, 0
    mov word [counter_xy], 0
    mov word [player_print_i], 0

_print_blank_screen_loop:
    mov word [es:bx], 0x0000
    inc bx
    inc bx
    inc word [counter_xy]
    cmp word [counter_xy], 1000
    jg _checkplayer1
    jmp _print_blank_screen_loop


; Prints player 1 paddle based on player1_X
_checkplayer1:
    mov bx, 0
    mov word [counter_xy], 0
    ; Reset Counters
    mov ax, 2
    imul ax, [player1_X]
    add bx, ax
_print_player1:
    
    mov word [es:bx], 0x2000 ; 
    inc bx
    inc bx
    inc word [player_print_i]
    cmp word [player_print_i], 10
    jg _checkplayer2
    jmp _print_player1



; Prints player 2 paddle based on player2_X
_checkplayer2:
    mov word [player_print_i], 0
    mov bx, 1920 ; bottom row
    mov ax, 2
    imul ax, [player2_X]
    add bx, ax
_print_player2:
    
    mov word [es:bx], 0x1000 ; 
    inc bx
    inc bx
    inc word [player_print_i]
    cmp word [player_print_i], 10
    jg _check_score
    jmp _print_player2

_check_score: ; logic from lab 7 ? NO
_print_score: ; Limit score at 5 haha, suck it
    
    ;p 1
    mov bx, 880
    mov cx, 0x0F00
    add cx, 48 ; for ascii
    add cx, word [score_1]
    mov word [es:bx], cx ; 

    ;p 2
    mov bx, 1040
    mov cx, 0x0F00
    add cx, 48 ; for ascii
    add cx, word [score_2]
    mov word [es:bx], cx ; 

_check_Puck:
_print_Puck:
    mov ax, word [ball_X]
    imul ax, 2
    mov bx, word [ball_Y]
    imul bx, 80
    add bx, ax
    mov word [es:bx], 0x3F00 ; 
    
    jmp _end




; wait for user input
_end:

	call _yield
	
	mov al, 0
	mov ah, 0x86
    mov cx, 0x0001
    mov dx, 0x0000
    int 0x15
	
	
	jmp _print_blank_screen
	;mov ah, 0x0 
	;int 0x16
    ;
	;mov ah, 0x4c
	;mov al, 0
	;int 0x21


Section .data

    screen_i:   dw 0    ; No use
    counter_xy: dw 0
    data_i:     dw 0
    player1_X:  dw 8    ; 0 - 40 - pad length
	player1_direction: dw 1
    player2_X:  dw 6    ; 0 - 40 - pad length
	player2_direction: dw -1
    ball_X:     dw 20    ; 0 - 40
    ball_Y:     dw 11    ; 0 - 25               (0 - 25) * 80
    player_print_i: dw 0
	ball_direction: dw 1
    score_1:    dw 1
    score_2:    dw 5



	current_task: db 0
	stacks: times (256 * 1) db 0 ; 31 fake stacks of size 256 bytes
	task_status: times 2 db 0 ; 0 means inactive, 1 means active
	stack_pointers: dw 0 ; the first pointer needs to be to the real stack !
					dw stacks + (256 * 1)
				