package main

import (
	"fmt"
	"time"
  "strconv"
  "strings"
  "os"
)

func main() {
  if len(os.Args) == 1 {
    usage()
  } else {
    runCommand(os.Args[1])
  }
}

// usage prints usage instructions
func usage() {
  var exeName = os.Args[0][strings.LastIndexAny(os.Args[0], "/\\") + 1:]
  
  fmt.Fprintf(os.Stderr, "%s plain\n", exeName)
  fmt.Fprintf(os.Stderr, "%s bidirectional\n", exeName)
  os.Exit(2)
}

// runCommand runs the specified command
func runCommand(cmd string) {
  switch strings.ToLower(cmd) {
    case "plain":
      averagePerf(plainQueue)
    case "bidirectional":
      averagePerf(bidirectionalQueue)
    default:
      usage()
  }
}

// averagePerf computes the average return value of fn over 10 runs
func averagePerf(fn func() int64) {
  const passes = 10
  var ms int64 = 0

  for i := 0; i < passes; i++ {
      var fnMs = fn()
      ms += fnMs
      fmt.Printf("%vms\n", fnMs)
  }

  fmt.Printf("\n %vms avg", ms / passes)
}

// plainQueue tests single-didrectional channel usage
func plainQueue() int64 {
	const numMessages = 1000000
	
  var count = 0
	var startTime = time.Now()
	var qout = make(chan string)
	var done = make(chan int64)
  	
	go func() {
    for {
      <- qout
      count++
      
      if count >= numMessages {
        done <- time.Now().Sub(startTime).Nanoseconds() / 1e6
        count = 0
      }
    }
	}()
  
  for i := 0; i < numMessages; i++ {
    qout <- "Msg " + strconv.Itoa(i)
  }
  
  return <-done
}

// bidirectionalQueue tests sending to a channel and awaiting a response
func bidirectionalQueue() int64 {
	const numMessages = 1000000
	
	var startTime = time.Now()
	var cstr = make(chan string)
	var cint = make(chan int)
  	
	go func() {
    for {
      i := <-cint
      cstr <- "Hey " + strconv.Itoa(i)      
    }
	}()
  
  for i := 0; i < numMessages; i++ {
    cint <- i
    <-cstr
  }
  
  return time.Now().Sub(startTime).Nanoseconds() / 1e6
}
