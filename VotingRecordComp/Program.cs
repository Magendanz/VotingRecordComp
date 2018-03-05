using System;
using System.Linq;
using System.Threading;
using VotingRecordComp.Services;

namespace VotingRecordComp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Percentage of time Lisa Brown and Ed Murray voted the same: {CompareVotingRecords(618, 1907):P1}");
            Console.WriteLine();

            Console.WriteLine($"Percentage of time Lisa Brown and Frank Chopp voted the same: {CompareVotingRecords(618, 1659):P1}");
            Console.WriteLine();

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        static double CompareVotingRecords(int primaryMemberId, int secondaryMemberId)
        {
            var billCount = 0;
            var floorVotes = 0;
            var opportunities = 0;
            var matches = 0;

            // Note: This must start on an odd year (long session)
            for (var year = 1995; year < 2013; year += 2)
            {
                var biennium = $"{year}-{(year + 1) % 100:D2}";
                Console.WriteLine($"Retrieving legislation for {biennium}...");

                // There's no GetLegislationByBiennium, so merge bill list for both years
                var bills = LegislationService.GetLegislationByYear(year);
                bills.AddRange(LegislationService.GetLegislationByYear(year + 1).Where(i => !bills.Contains(i)));

                Console.WriteLine($"Analyzing roll call votes for {bills.Count} bills...");
                foreach (var bill in bills)
                {
                    string primaryVote = null, secondaryVote = null;

                    ++billCount;
                    var votes = LegislationService.GetRollCalls(bill.Biennium, bill.BillNumber).OrderBy(i => i.VoteDate);

                    foreach (var vote in votes)
                    {
                        ++floorVotes;
                        primaryVote = vote.Votes.FirstOrDefault(i => i.MemberId == primaryMemberId)?.VOte ?? primaryVote;
                        secondaryVote = vote.Votes.FirstOrDefault(i => i.MemberId == secondaryMemberId)?.VOte ?? secondaryVote;
                    }

                    // Note that we end up comparing the first floor vote on the bill by that member
                    if (primaryVote != null && secondaryVote != null)
                    {
                        ++opportunities;
                        if (primaryVote == secondaryVote)
                        {
                            ++matches;
                            Console.Write("+");
                        }
                        else
                            Console.Write("-");
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine($"Bills analyzed during this period: {billCount}");
            Console.WriteLine($"Roll call floor votes analyzed: {floorVotes}");
            Console.WriteLine($"Bills that received floor votes from both members: {opportunities}");
            Console.WriteLine($"Bills where their votes matched: {matches}");

            return (double)matches / opportunities;
        }
    }
}
