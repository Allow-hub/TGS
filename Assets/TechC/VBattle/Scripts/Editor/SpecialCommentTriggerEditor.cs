using UnityEditor;
using TechC.CommentSystem;

namespace TechC.EditorTools
{
    [CustomEditor(typeof(SpecialCommentTrigger))]
    public class SpecialCommentTriggerEditor : PolymorphicListEditor<SpecialCommentTrigger, ICommentAbility>
    {
        protected override string PropertyName => "abilities";
    }
}
