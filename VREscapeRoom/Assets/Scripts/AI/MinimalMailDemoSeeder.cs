using UnityEngine;

public class MinimalMailDemoSeeder : MonoBehaviour
{
    public MailflowManager mf;

    void Awake()
    {
        if (mf == null) mf = GetComponent<MailflowManager>();
        if (mf.inbox == null || mf.inbox.Length == 0)
        {
            mf.inbox = new MailflowManager.Mail[]{
                new MailflowManager.Mail{ id="1", subject="Student absence", body="Hi, my child was sick yesterday. Can it be excused?", expected="Acknowledge receipt; mark absence excused for the stated date; mention any needed documentation if policy requires it." },
                new MailflowManager.Mail{ id="2", subject="Schedule change", body="Can we move the parent-teacher meeting from Friday to Monday?", expected="Offer alternative slots; confirm new date/time or propose options; be polite and clear." },
                new MailflowManager.Mail{ id="3", subject="HR complaint", body="The air conditioner in Room 305 is broken.", expected="Acknowledge; create/confirm a ticket; give ETA or next steps; escalate to facilities." },
            };
        }
    }

    void Start() { mf.NextMail(); }
}
