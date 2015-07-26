package main

import (
	"fmt"
	"time"
  "strconv"
)

func main() {
	demoPerfBidirectionalChannels()
}

func demoPerfBidirectionalChannels() {
	fmt.Println("Bidirectional channels")
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
  
  fmt.Printf("Done in %vms\n", time.Now().Sub(startTime).Nanoseconds() / 1e6)
}
