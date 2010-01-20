function argsToArray(args) {
  var result = []
  for ( var i=0; i<args.length; i++ )
    result.push(args[i])
  return result
}
Object.prototype.curry = function() {
  var f = this
  var newArgs = []
  for ( var i=0; i<arguments.length; i++ )
    newArgs.push(arguments[i])
  return function() {
    for ( var i=0; i<arguments.length; i++ )
      newArgs.push(arguments[i])
    return f.apply(this, newArgs)
  }
}
Array.prototype.where = Array.prototype.filter
Array.prototype.select = Array.prototype.map
Array.prototype.skip = function(n) {
  var result = []
  for ( var i=n; i<this.length; i++ )
    result.push(this[i])
  return result    
}
Array.prototype.take = function(n) {
  var result = []
  for ( var i=0; i<this.length && i<n; i++ )
    result.push(this[i])
  return result    
}
Array.prototype.aggregate = function(init, f) {
  argsToArray(this).forEach(function(e) {
    init = f(init, e)
  })
  return init
}
Array.prototype.sum = Array.prototype.aggregate.curry(0, function(a,b){ return a + b })
Array.prototype.each_cons = function(n) {
  var result = []
  for ( var i=0; i<=this.length-n; i++ )
    result.push(argsToArray(this).skip(i).take(n))
  return result    
}
Array.prototype.indexOfPred = function(f) {
  for ( var i=0; i<this.length; i++ )
    if (f(this[i]))
      return i
  return -1
}
