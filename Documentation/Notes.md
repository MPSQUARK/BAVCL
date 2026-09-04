# Notes

## Statistics Module:

context:

A lot of stats operations rely on data being sorted first. So computing currently varius stats properties attempt to re-sort the data each time.

Proposals:

1. Implement a property which can be checked that a data set is sorted, and if it is then skip.

2. use a Scoped context, to sort the data once, compute stats inside the scope. This also allows for the option to leave original data unsorted - if operating on a copy.
