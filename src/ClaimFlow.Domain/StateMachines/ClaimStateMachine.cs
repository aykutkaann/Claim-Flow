using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using Stateless;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.StateMachines
{
    public class ClaimStateMachine
    {
        private readonly StateMachine<ClaimStatus, ClaimTrigger> _machine;
        private readonly Claim _claim;
        
        public ClaimStateMachine(Claim claim)
        {
         
            
            _claim = claim;

            _machine = new StateMachine<ClaimStatus, ClaimTrigger>(
                () => _claim.Status,
                s => _claim.Status = s);

            ConfigureTransitions();
        }

        private void ConfigureTransitions()
        {

            // 1 - Submit stage

            _machine.Configure(ClaimStatus.Submitted)
                .Permit(ClaimTrigger.StartReview, ClaimStatus.UnderReview);

            // 2 - UnderReview Stage

            _machine.Configure(ClaimStatus.UnderReview)
                .Permit(ClaimTrigger.RequestDocuments, ClaimStatus.DocumentsRequested)
                .Permit(ClaimTrigger.StartInvestigation, ClaimStatus.UnderInvestigation)
                .Permit(ClaimTrigger.Approve, ClaimStatus.Approved)
                .Permit(ClaimTrigger.Reject, ClaimStatus.Rejected);

            // 3 - DocumentREquested

            _machine.Configure(ClaimStatus.DocumentsRequested)
                .Permit(ClaimTrigger.StartReview, ClaimStatus.UnderReview);

            // 4 - UnderInvestigation

            _machine.Configure(ClaimStatus.UnderInvestigation)
                .Permit(ClaimTrigger.Approve, ClaimStatus.Approved)
                .Permit(ClaimTrigger.Reject, ClaimStatus.Rejected);

            // 5 - Approved

            _machine.Configure(ClaimStatus.Approved)
                .Permit(ClaimTrigger.SchedulePayment, ClaimStatus.PaymentScheduled);

            // 6 - PaymentScheduled

            _machine.Configure(ClaimStatus.PaymentScheduled)
                .Permit(ClaimTrigger.ConfirmPayment, ClaimStatus.Paid);

            // 7 - Paid

            _machine.Configure(ClaimStatus.Paid)
                .Permit(ClaimTrigger.Close, ClaimStatus.Closed);

            // 8 - Rejected

            _machine.Configure(ClaimStatus.Rejected)
                .Permit(ClaimTrigger.FileAppeal, ClaimStatus.Appeal);

            // 9 - Appeal

            _machine.Configure(ClaimStatus.Appeal)
                .Permit(ClaimTrigger.StartReview, ClaimStatus.UnderReview); // re enters the cycle

            // 10 - Closed (terminal)
            _machine.Configure(ClaimStatus.Closed);



        }

        public void Fire(ClaimTrigger trigger) => _machine.Fire(trigger);


        public bool CanFire(ClaimTrigger trigger) => _machine.CanFire(trigger);

        public IEnumerable<ClaimTrigger> GetPermittedTriggers() => _machine.PermittedTriggers;


    }
}
