/* Echo Server: an example usage of EzNet
 * (c) 2016, Bob Jones University
 */
#include <stdbool.h>    // For access to C99 "bool" type
#include <stdio.h>      // Standard I/O functions
#include <stdlib.h>     // Other standard library functions
#include <string.h>     // Standard string functions
#include <errno.h>      // Global errno variable

#include <stdarg.h>     // Variadic argument lists (for blog function)
#include <time.h>       // Time/date formatting function (for blog function)

#include <unistd.h>     // Standard system calls
#include <signal.h>     // Signal handling system calls (sigaction(2))

#include "eznet.h"      // Custom networking library
#include <bsd/string.h>
#include <limits.h>
#include <pthread.h>
#include "utils.h"
#define MAX_HEADERS 10
#define ARRAY_LENGTH_BYTES 1024//bytes. used with malloc
#define LINE_LENGTH 70

char MASTER_PATH[100];//var to hold root directory
int MAX_CLIENTS = 5; //the max number of threads
pthread_t *threads = NULL; //array to hold client thread
int ACTIVE_THREADS = 0; //the number of client threads in use
pthread_mutex_t threads_mutex; //MUTEX to protect thread array

//EXTEN holds known file extensions
char EXTEN[8][6] = { 
    {'h','t','m','l','\0'},
    {'h','t','m','\0'},
    {'g','i','f','\0'},
    {'j','p','g','\0'},
    {'j','p','e','g','\0'},
    {'p','n','g','\0'},
    {'c','s','s','\0'},
    {'p','l','a','i','n','\0'}
    
};
//CONTENT_TYPE holds correct heading for each EXTEN extension
char CONTENT_TYPE [9][26] = {
    {'t','e','x','t','/','h','t','m','l','\0'},
    {'t','e','x','t','/','h','t','m','l','\0'},
    {'i','m','a','g','e','/','g','i','f','\0'},
    {'i','m','a','g','e','/','j','p','e','g','\0'},
    {'i','m','a','g','e','/','j','p','e','g','\0'},
    {'i','m','a','g','e','/','p','n','g','\0'},
    {'t','e','x','t','/','c','s','s','\0'},
    {'t','e','x','t','/','p','l','a','i','n','\0'},
    {'a','p','p','l','i','c','a','t','i','o','n','/','o','c','t','e','t','-','s','t','r','e','a','m','\0'}
};




typedef struct http_request {
    char *verb;
    char *path;
    char *version;

} http_request_t;


// GLOBAL: settings structure instance
struct settings {
    const char *bindhost;   // Hostname/IP address to bind/listen on
    const char *bindport;   // Portnumber (as a string) to bind/listen on
} g_settings = {
    .bindhost = "localhost",    // Default: listen only on localhost interface
    .bindport = "2000",         // Default: listen on TCP port 5000
};

// Parse commandline options and sets g_settings accordingly.
// Returns 0 on success, -1 on false...
int parse_options(int argc, char * const argv[]) {
    int ret = -1; 

    char op;
    while ((op = getopt(argc, argv, "h:p:r:w:")) > -1) {
        switch (op) {
            case 'h':
                g_settings.bindhost = optarg;
                break;
            case 'p':
                g_settings.bindport = optarg;
                break;
            case 'r':
                if(strlen(optarg) < 100){
                strlcpy(MASTER_PATH, optarg, strlen(optarg)+1);
                }
                else
                {
                    MASTER_PATH[0] = '\0';
                }
                break;
            case 'w':
                MAX_CLIENTS = atoi(optarg);
                break;
            default:
                // Unexpected argument--abort parsing
                goto cleanup;
        }
    }

    ret = 0;
cleanup:
    return ret;
}

// GLOBAL: flag indicating when to shut down server
volatile bool server_running = false;

// SIGINT handler that detects Ctrl-C and sets the "stop serving" flag
void sigint_handler(int signum) {
    blog("Ctrl-C (SIGINT) detected; shutting down...");
    server_running = false;
}

// Returns 1 on success,
// -1 on invalid HTTP request,
// -2 on I/O error, 
// -2 on malloc failure
// -1 File path too long
// -4 WRONG VERB
int parseHttp(FILE *in, http_request_t **request) 
{
    
    http_request_t *req = NULL;
    int rc = 0;
    char *line; //used to read from file
    size_t n = 0;
    char *memory = NULL; //used for allocating memory
    char *saveptr = NULL;
    int flag = 1;
    char *save = NULL;
    int size = 256;
    // TODO: Allocate memory for req
    if((req = malloc(ARRAY_LENGTH_BYTES)) == NULL){ //allocate memory for the struct
        rc = -2;
        goto cleanup;
    }

    //making sure the starting values are correct
    req -> verb = NULL; 
    req -> path = NULL;
    req -> version = NULL;

    line = calloc(size, sizeof(char));
    char *e = fgets(line,size-1,in); //read first line
    printf("\nREQUEST FROM CLIENT: %s\n", line);

    // TODO: Read HTTP request from <in>
    if(e == NULL){ //if null
        rc = -1;
        goto cleanup;
    }

    if(line[strlen(line)-1] != '\n')
    {
        rc = -6;
        goto cleanup;
    }
    if(strcmp(line, "\r\n") == 0)
    {
        rc = -1;
        goto cleanup;
    }
    if(strchr(line,' ') == NULL)
    {
        rc = -1;
        goto cleanup;
    }
    char *re;
    re = strtok_r(line, " ", &save); //parse the string, I could not get strtok_r to work
        if(strcmp("GET",re) == 0) //check verb
        {
            //malloc space for verb string
            memory = calloc(4,sizeof(char));
            if(memory == NULL)
            {
                rc = -3;
                free(memory);
                goto cleanup;
            }
            strlcpy(memory, re, 10);
            req->verb = memory;
        }
        else
        {
            rc = -4;
            goto cleanup;
        } 
        re = strtok_r(NULL, " ",&save); //parse again, assumptiion that the first line contains 3 arguments
        if(re[0] == '/' || re[0] == '.' || (re[0] == '.' && re[1] == '.'))// check vailid path (WEAK CHECK)
        {
           
                memory = calloc(256,sizeof(char));
     
            if(memory == NULL)
            {
                rc = -2;
                free(memory);
                goto cleanup;
            }
            if(strlen(re) >= 256)
            {
                rc = -1;
                free(memory);
                goto cleanup;
            }
            if(re[0]=='.'){
            re = realpath(re, memory);
            }else
            {
                strlcpy(memory, re, strlen(re)+1);
            }
            req->path = memory;
        }else
        {
            rc = -1;
            goto cleanup;
        }

        re = strtok_r(NULL, " ",&save);
        char tmp[10]; 
        strlcpy(tmp, re, 9);
        if(strcmp("HTTP/1.1", tmp ) == 0 || strcmp("HTTP/1.0", tmp) == 0) //check the version number. added support for 1.1 because of warproxy
        {
            memory = calloc(9, sizeof(char));
            if(memory == NULL)
            {
                rc = -2;
                goto cleanup;
            }
            strlcpy(memory, re, 9);
            req->version = memory;
            
        }else
        {
            rc = -1;   
            goto cleanup;
        }
        

    // TODO: On success, return req
   
    *request = req;
    rc = 1;
    goto get_headers;
    

cleanup:
    //free all memory in req
    free(req->verb);
    free(req->path);
    free(req->version);
    free(req);  // It's OK to free() a NULL pointer 
    
if(rc == -6){
    free(line);
    return -1;
}

//read all the headers and ignore
get_headers:
    
      free(line);
        while(flag)
        {
            getline(&saveptr, &n, in);
           
            if(strcmp(saveptr, "\r\n") == 0 || strcmp(saveptr, "\n") == 0)
            {
                flag = 0; 
            }

        free(saveptr);
        saveptr = NULL;

        } //ingnore all headers
        
return rc;

}

//returns 0 on succsess
//returns -1 if file does not exist
// returns -2 if file exist, but user does not have premission
// returns -3 if path is greater than 256
FILE *GetFile(char *path, int *rc)
{
    FILE *file = NULL; 
   
    if((strlen(path)+strlen(MASTER_PATH)) > 256)
    {
        *rc = -3;
        return NULL;
    }

    char new_path[256];
        //printf("\npath requrested: %s\n",path);
        strlcpy(new_path, MASTER_PATH, strlen(MASTER_PATH)+1);
        //printf("\nMASTER PATH = %s\n",new_path);
        strlcat(new_path, path, strlen(MASTER_PATH)+strlen(path)+1);
    // else
    // {
    //     strlcpy(new_path, path, strlen(path) + 1);
    // }

    //printf("\nFILE PATH = %s\n", new_path);

    file = fopen(new_path, "r+");
  
    if(file == NULL)
    {
        *rc = -1;
    }
    else
    {
        *rc = 0;
    } 

    if( fopen(path, "r") != NULL && *rc != 0)
    {
        *rc = -2;
    }

    return file;
}

//Prints a response to file pointed to by stream
// ec is the error code to send
void response(int ec, FILE **stream)
{
    switch(ec)
    {
        
        case 400:
                fputs("HTTP/1.0 400 bad request\r\n", *stream);
                fputs("Content-Type: text/plain\r\n", *stream);
                fputs("\r\n", *stream);

                fputs("400 Bad request\r\n", *stream);
                printf("\nERROR----400\n");
                break;
        case 403:
                fputs("HTTP/1.0 403 Forbidden\r\n", *stream);
                fputs("Content-Type: text/plain\r\n", *stream);
                fputs("\r\n", *stream);

                fputs("403 Forbidden\r\n", *stream);
                printf("\nERROR----403\n");
                break;
        case 404:
                fputs("HTTP/1.0 404 Not Found\r\n", *stream);
                fputs("Content-Type: text/plain\r\n", *stream);
                fputs("\r\n", *stream);

                fputs("404 NOT FOUND\r\n", *stream);
                printf("\nERROR----404\n");
                break;  
        case 501:
                fputs("HTTP/1.0 501 unsupported request\r\n", *stream);
                fputs("Content-Type: text/plain\r\n", *stream);
                fputs("\r\n", *stream);

                fputs("501 unsupported request\r\n", *stream);
                printf("\nERROR----501\n");
                break;
        default:
                fputs("HTTP/1.0 500 server error\r\n", *stream);
                fputs("Content-Type: text/plain\r\n", *stream);
                fputs("\r\n", *stream);

                fputs("500 Server Error\r\n", *stream);
                printf("\nERROR----500\n");
                break;
    }

    fflush(*stream);
}

//examines the bytes following the last '.' in a string
// and assigns a proper content-type
char *get_extension(char* path)
{
    char *extension;
    extension = strrchr(path ,(int)'.');
    extension++;
    for(int i = 0; i < 8; i++)
    {
        if(strcmp(extension, EXTEN[i]) == 0)
        {
            return CONTENT_TYPE[i];
        }
    }

    return CONTENT_TYPE[8];
}

void serve_file(FILE **file, FILE **stream, char *extension)
{
    
    fputs("Content-Type: ",*stream);
    fputs(extension,*stream);
    fputs("\r\n", *stream);
    fputs("\r\n", *stream);
    //printf("\n---GETING A LINE IN HANDLE CLIENT---\n");
    unsigned char *response = calloc(10, sizeof(unsigned char));
    //unsigned char response[5];
    while(fread(response , sizeof(unsigned char), 10, *file) > 4)
    {
        fwrite(response, sizeof(unsigned char), 10, *stream);
        //fputs(response, *stream);
    }
    free(response);
    fclose(*file);
    printf("\nGREAT-------200\n");
}


// takes a thread and frees it for use by another client
void free_thread(pthread_t *thread)
{
    bool flag = false;
    for(int i = 0; i < MAX_CLIENTS; i++)
    {
        pthread_mutex_lock(&threads_mutex);
        if(&threads[i] == thread)
        {
            threads[i] = 0;
            flag = true;
            ACTIVE_THREADS--;
            //printf("active threads %d->%d", 1 + ACTIVE_THREADS, ACTIVE_THREADS);
            
        }
        pthread_mutex_unlock(&threads_mutex);
        if(flag) return;
    }
}


// Connection handling logic: reads/echos lines of text until error/EOF,
// then tears down connection.
void *handle_client(void *client) {
    FILE *stream = NULL;
    http_request_t *req = NULL;
    struct client_info request = *((struct client_info*)client);
    // Wrap the socket file descriptor in a read/write FILE stream
    // so we can use tasty stdio functions like getline(3)
    // [dup(2) the file descriptor so that we don't double-close;
    // fclose(3) will close the underlying file descriptor,
    // and so will destroy_client()]
    if ((stream = fdopen(dup(request.fd), "r+"))== NULL) {
        perror("unable to wrap socket");
        goto cleanup;
    }
    
    FILE *file = NULL;
    int rc = 0;
    char *answer = NULL;
    switch(parseHttp(stream, &req))
    {
        case 1:
            file = GetFile(req->path, &rc);
            switch(rc){
                case 0:                 
                    answer = "HTTP/1.0 200 OK\r\n";
                    fputs(answer, stream);
                    char *extension = get_extension(req->path); 
                    serve_file(&file, &stream, extension);
                    break;
                case -1:
                    response(404,&stream);
                    break;  
                case-2:
                    response(403, &stream);
                    break;
            }
            free(req->verb);
            free(req->path);
            free(req->version);
            free(req);
            break;
        case -1:
            response(400, &stream);
            goto cleanup;
        case -4:
            response(501, &stream);
            goto cleanup;
        default:
            response(500,&stream); 
            goto cleanup;       
    }

cleanup:
    // Shutdown this client
    if (stream) fclose(stream);
    free_thread(request.thread);
    destroy_client_info(&request);
    printf("\tSession ended.\n");
    printf("---------COMPLETED A REQUEST----------\n\n");
    return NULL;
}

//returns a true if a thread is avalible
//false if no thread ready
bool get_thread(pthread_t **client)
{
    bool flag = false;
    for(int i = 0; i < MAX_CLIENTS; i++)
    {
        pthread_mutex_lock(&threads_mutex);
        if(threads[i] == 0)
        {
            *client = &threads[i];
            flag = true;
            ACTIVE_THREADS++;
           // printf("active threads %d->%d", ACTIVE_THREADS - 1, ACTIVE_THREADS);
            pthread_mutex_unlock(&threads_mutex);
            break;
        }
        pthread_mutex_unlock(&threads_mutex);
    }
    return flag;
}



int main(int argc, char **argv) {
    int ret = 1;

    // Network server/client context
    int server_sock = -1;

    pthread_t *thread;
    // Handle our options
    if (parse_options(argc, argv)) {
        printf("usage: %s [-p PORT] [-h HOSTNAME/IP]\n", argv[0]);
        goto cleanup;
    }

    if(MASTER_PATH[0] == '\0')
    {
        getcwd(MASTER_PATH, (size_t)30);
        //printf("\nWORKING DIRECTORY: %s\n", MASTER_PATH);
    }
    printf("\n\n the Path is %s\n\n", MASTER_PATH);

    //int size = MAX_CLIENTS * (int)sizeof(pthread_t);
    threads = calloc(MAX_CLIENTS,sizeof(pthread_t));
    //threads = malloc(size);

    pthread_mutex_init(&threads_mutex, NULL);

    // Install signal handler for SIGINT
    struct sigaction sa_int = {
        .sa_handler = sigint_handler
    };
    if (sigaction(SIGINT, &sa_int, NULL)) {
        LOG_ERROR("sigaction(SIGINT, ...) -> '%s'", strerror(errno));
        goto cleanup;
    }


    // Start listening on a given port number
    server_sock = create_tcp_server(g_settings.bindhost, g_settings.bindport);
    if (server_sock < 0) {
        perror("unable to create socket");
        goto cleanup;
    }
    blog("Bound and listening on %s:%s", g_settings.bindhost, g_settings.bindport);

    server_running = true;
    while (server_running) {
        struct client_info client;
        printf("\n----------STARTING A REQUEST----------\n");
        blog(" Active Clients %d", ACTIVE_THREADS);
        // Wait for a connection on that socket
        if (wait_for_client(server_sock, &client)) {
            // Check to make sure our "failure" wasn't due to
            // a signal interrupting our accept(2) call; if
            // it was  "real" error, report it, but keep serving.
            if (errno != EINTR) { perror("unable to accept connection"); }
        } else {
            blog("connection from %s:%d", client.ip, client.port);
            if(get_thread(&thread))
            {
             
            client.thread = thread;
            
            pthread_create(thread, NULL, handle_client, &client); // Client gets cleaned up in here
         
            }

            else
            {
                printf("\nNO THREAD READY\n");
                destroy_client_info(&client);
            }
        }
        
        
    }
    ret = 0;

cleanup:
    if (server_sock >= 0) close(server_sock);
    free(threads);
    return ret;
}