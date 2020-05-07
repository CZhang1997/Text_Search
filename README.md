# Text_Search
# C#
# Winform app

# Multithread Background Worker Example

# Version 1
Example of using 50 threads to read from a text file and process then on the same time.
Notes: not sure if many thread reading from the same file on the same time is okay.

# Version 2
Example of using two threads. One thread read, process text and add target line into a queue. Second thread poll from the queue and add line to UI for display.

# Version 3
Example of using one extra thread, other than the main thread. The thread will read and process each line of text and add to UI. Final Version also contains a progess bar, and copy line of text from UI.
