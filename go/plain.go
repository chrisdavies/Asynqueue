package main

import (
	"fmt"
	"time"
  "strconv"
)

func main() {
	demoPerfPlainChannels()
}

func demoPerfPlainChannels() {
	fmt.Println("Plain channels")
	const numMessages = 1000000
	
  var count = 0
	var startTime = time.Now()
	var qout = make(chan string)
	var done = make(chan int)
  	
	go func() {
    for {
      <- qout
      count++
      
      if count >= numMessages {
        fmt.Printf("Done in %vms\n", time.Now().Sub(startTime).Nanoseconds() / 1e6)
        done <- 1
        count = 0
      }
    }
	}()
  
  for i := 0; i < numMessages; i++ {
    qout <- "Msg " + strconv.Itoa(i)
  }
  
  <- done
}
