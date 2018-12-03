bits 16

org 0x0

SECTION .text
_main:
; ====================== Start Buffer ====================== ;
	cli
	mov ax, 0
	mov es, ax

	mov dx, [es:0x9*4]
	mov [previous9], dx
	mov ax, [es:0x9*4+2]
	mov [previous9+2], ax

	mov dx, keyboard
	mov [es:0x9*4], dx
	mov ax, cs 
	mov [es:0x9*4+2], ax
	sti


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
  
; ====================== TASK #3 ====================== ;
; ====================== AI      ====================== ;

  ;mov dx _AI
  ;call _spawn_new_task

; ====================== TASK #4 ====================== ;
; ====================== SCORING  ====================== ;
  ; If ball is in row 0
  ;		PLayer Score
  ; if ball is in row 24
  ;		AI SCORE

;  mov dx, _score_keeper
  ;call _spawn_new_task
  
; ====================== TASK #1 ====================== ;
; ====================== Input  ====================== ;
;_update_game_state
	loop_input:
	
	call _updateAI
	call _CheckInput
	call _updateball
	
	call _yield
	jmp loop_input
  

	
			

; ====================== INPUT Logic ====================== ;
	_CheckInput:
		; A
		;	1e & 9e
		; D
		;	20 & a0



	_seeInput:
		cmp byte [number], 0x1e
		je _moveLeft
		cmp byte [number], 0x20
		je _moveRight
		ret
	_moveLeft:
		cmp word [player_X], 0
		je _finish
		sub word [player_X], 1
		jmp _finish
	_moveRight:
		cmp word [player_X], 29
		je _finish
		add word [player_X], 1
		je _finish

	_finish:
		ret
	
	keyboard:
		push ax
		in al, 0x60
		mov [number], al
		mov al, 0x20
		out 0x20, al
		pop ax
		iret

	; ====================== AI Logic ====================== ;
	_updateAI:
		push ax
		mov ax, [AI_X]
		cmp ax, 29
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
		mov ax, word [AI_direction]
		add [AI_X], ax
		pop ax
		ret

		_upper_limtX:
			mov word [AI_direction], -1
			jmp _updateX
			
		_lower_limtX:
			mov word [AI_direction], 1
			jmp _updateX


; ====================== Ball(er) Logic ====================== ;
	_updateball: ; Add better logic later
		push ax
		push bx
		push cx
		mov ax, word [ball_Y]
		mov bx, word [player_X]
		mov cx, word [ball_X]
		mov word [CheckPaddle_i], 0

	_Check_Ball_Near_Player:
		cmp ax, 23
		je _Check_Player_Hit
		jmp _Check_Ball_Near_AI

	_Check_Player_Hit:
		cmp cx, bx
		je _CheckPlayerLeft
		inc word [CheckPaddle_i]
		inc bx
		cmp word[CheckPaddle_i], 11
		jl _Check_Player_Hit
		jmp _Check_Ball_Near_AI

		_CheckPlayerLeft:
			cmp word[CheckPaddle_i], 3
			jg _CheckPlayerMiddle
			mov word [ball_direction_x], -1
			jmp _hitPlayerPaddle				
		_CheckPlayerMiddle:
			cmp word[CheckPaddle_i], 6
			jg _CheckPlayerRight
			mov word [ball_direction_x], 0
			jmp _hitPlayerPaddle				
		_CheckPlayerRight:
			mov word [ball_direction_x], 1
			jmp _hitPlayerPaddle		

_hitPlayerPaddle:
	
	mov word [ball_direction_y], -1	

_Check_Ball_Near_AI:
		mov word [CheckPaddle_i], 0
		mov bx, word [AI_X]

		cmp ax, 1
		je _Check_AI_Hit
		jmp _Check_Player_Score
	_Check_AI_Hit:
		cmp cx, bx
		je _CheckAILeft
		inc word [CheckPaddle_i]
		inc bx
		cmp word [CheckPaddle_i], 11
		jl _Check_AI_Hit
		jmp _Check_Player_Score

		_CheckAILeft:
			cmp word[CheckPaddle_i], 3
			jg _CheckAIMiddle
			mov word [ball_direction_x], -1
			jmp _hitAIPaddle
		_CheckAIMiddle:
			cmp word[CheckPaddle_i], 6
			jg _CheckAIRight
			mov word [ball_direction_x], 0
			jmp _hitAIPaddle	
		_CheckAIRight:
			mov word [ball_direction_x], 1
			jmp _hitAIPaddle	

	_hitAIPaddle:
		mov word [ball_direction_y], 1
		
	_Check_Player_Score:
		cmp ax, 0
		jne _Check_AI_Score
		jmp _Player_Score

	_Check_AI_Score:
		cmp ax, 24	
		jne _CheckLeftWall
		jmp _AI_Score		
	_CheckLeftWall:
		mov ax, word [ball_X]
		cmp ax, 0	
		je _BounceToRight
		jmp _CheckRightWall
	_BounceToRight:
		mov word [ball_direction_x], 1
		jmp _update_y
	_CheckRightWall:
		cmp ax, 39	
		je _BounceToLeft
		jmp _update_y
	_BounceToLeft:
		mov word [ball_direction_x], -1
		jmp _update_y
		cmp ax, 24	
		jne _CheckLeftWall
	_update_y:
		mov ax, word [ball_direction_y]
		add word [ball_Y], ax
		mov ax, word [ball_direction_x]
		add word [ball_X], ax
		pop cx
		pop bx
		pop ax
		ret
	
	_Player_Score:
		inc word [score_PLAYER]
		mov word [ball_direction_y], 1
		jmp _reset_ball
	
	_AI_Score:
		inc word [score_AI]
		mov word [ball_direction_y], -1
		jmp _reset_ball

	_reset_ball:
		mov word [ball_direction_x], 0
		mov word [ball_X], 20
		mov word [ball_Y], 11
	


; ====================== Tasks Logic ====================== ;
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
; ====================== TASK 2      ====================== ;
; ====================== Print Logic ====================== ;
; ==== Set Screen to Black ==== ;
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
    jg _checkAI
    jmp _print_blank_screen_loop


; ==== Print Top Player (AI) ==== ; 
_checkAI:
    mov bx, 0
    mov word [counter_xy], 0
    ; Reset Counters
    mov ax, 2
    imul ax, [AI_X]
    add bx, ax
    
_print_AI:
    mov word [es:bx], 0x2000 ; 
    inc bx
    inc bx
    inc word [player_print_i]
    cmp word [player_print_i], 10
    jg _checkplayer
    jmp _print_AI



; ==== Print Bottom Player (Person) ==== ; 
_checkplayer:
    mov word [player_print_i], 0
    mov bx, 1920 ; bottom row
    mov ax, 2
    imul ax, [player_X]
    add bx, ax
_print_player:
    
    mov word [es:bx], 0x1000 ; 
    inc bx
    inc bx
    inc word [player_print_i]
    cmp word [player_print_i], 10
    jg _print_score
    jmp _print_player

; ==== Print Scores ==== ; 
_print_score: ; Limit score at 5 haha, suck it
    
    ;p 1
    mov bx, 880
    mov cx, 0x0F00
    add cx, 48 ; for ascii
    add cx, word [score_AI]
    mov word [es:bx], cx ; 

    ;p 2
    mov bx, 1040
    mov cx, 0x0F00
    add cx, 48 ; for ascii
    add cx, word [score_PLAYER]
    mov word [es:bx], cx ; 

; ==== Print Ball ==== ; 
_print_Puck:
    mov ax, word [ball_X]
    imul ax, 2
    mov bx, word [ball_Y]
    imul bx, 80
    add bx, ax
    mov word [es:bx], 0x3F00 ;     
    jmp _end

_end:

	call _yield
	
	mov al, 0
	mov ah, 0x86
  mov cx, 0x0001
  mov dx, 0x0000
  int 0x15
  jmp _print_blank_screen

; ====================== TASK #3 ====================== ;
; ====================== AI  ====================== ;

  _AI:
  ; move AI paddle if ball is 10 pixels away from scoring 
  ; call _yeild

; ====================== TASK #4 ====================== ;
; ====================== SCORING  ====================== ;
  ; If ball is in row 0
  ;		PLayer Score
  ; if ball is in row 24
  ;		AI SCORE
	_Score_keeper:
  
  ;call _yeild

  
  
Section .data

  screen_i:   dw 0    ; No use
  counter_xy: dw 0
  data_i:     dw 0
  AI_X:  dw 0    ; 0 - 25ish
  AI_direction: dw 1
  player_X:  dw 11    ; 0 - 25ish
  player_direction: dw -1
  ball_X:     dw 20    ; 0 - 40
  ball_Y:     dw 11    ; 0 - 25               (0 - 25) * 80
  player_print_i: dw 0
  ball_direction_y: dw 1
  ball_direction_x: dw 0
  score_AI:    dw 0
  score_PLAYER:    dw 0
  CheckPaddle_i: dw 0
	number: db 0
	previous9: dd 0
	bufferOpen: db 0

	current_task: db 0
	stacks: times (256 * 1) db 0 ; 31 fake stacks of size 256 bytes
	task_status: times 2 db 0 ; 0 means inactive, 1 means active
	stack_pointers: dw 0 ; the first pointer needs to be to the real stack !
					dw stacks + (256 * 1)
				