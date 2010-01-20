load("lib.js")

function solve(runeName, startNightmare, startHell, runes) {
  var sol = { ctr: 0, ctrCalc: 0, ctrMemo: 0 }
  var rune = runes.indexOfPred(function(e){ return e.name == runeName })

  runes[0].value = 1
  runes.each_cons(2).forEach(function(r) { r[1].value = r[0].value * r[0].next })
 
  function divide(value, hell, mare) {
    sol.ctr++
    if (hell + mare == 0)
      return value >= runes[rune].value ? 1 : 0

    var memo = value + " " + (hell + mare)
    if (sol[memo] == undefined) {
      if (++sol.ctrCalc % 10000 == 0)
        print(sol.ctrCalc + " cases calculated so far")
      sol[memo] = runes.select(function(e, i) { return { mult: hell > 0 ? e.hell : e.mare, addition: i <= rune ? e.value : 0 }})
                       .where(function(e) { return e.mult > 0 })
                       .select(function(e) { return e.mult * divide(value + e.addition, hell - (hell > 0 ? 1 : 0), mare - (hell > 0 ? 0 : 1)) })
                       .sum()
    } else
      sol.ctrMemo++
    return sol[memo]
  }

  sol.prob = divide(0, startHell, startNightmare)
  return sol
}

d = new Date()
millis = d.getMilliseconds() + 1000 * d.getSeconds() + 60000 * d.getMinutes()
eval("input = " + readFile("runes.json"))
print("Trying to find the rune '" + input.rune + "' using hellforge runs; " + input.nightmare + " in nightmare and " + input.hell + " in hell...")
res = solve(input.rune, input.nightmare, input.hell, input.runes)
d = new Date()
print("Done! The search took " + (d.getMilliseconds() + 1000 * d.getSeconds() + 60000 * d.getMinutes() - millis)+ " milliseconds.")
print("Total recursive calls:   " + res.ctr)
print("Calls using memoization: " + res.ctrMemo)
print("Calls using calculation: " + res.ctrCalc)
print()
print("And the probability of success is: " + (res.prob*100).toFixed(1) + "%")
