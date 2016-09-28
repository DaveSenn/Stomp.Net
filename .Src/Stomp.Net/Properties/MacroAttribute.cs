#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Allows specifying a macro for a parameter of a <see cref="SourceTemplateAttribute">source template</see>.
    /// </summary>
    /// <remarks>
    ///     You can apply the attribute on the whole method or on any of its additional parameters. The macro expression
    ///     is defined in the <see cref="MacroAttribute.Expression" /> property. When applied on a method, the target
    ///     template parameter is defined in the <see cref="MacroAttribute.Target" /> property. To apply the macro silently
    ///     for the parameter, set the <see cref="MacroAttribute.Editable" /> property value = -1.
    /// </remarks>
    /// <example>
    ///     Applying the attribute on a source template method:
    ///     <code>
    /// [SourceTemplate, Macro(Target = "item", Expression = "suggestVariableName()")]
    /// public static void forEach&lt;T&gt;(this IEnumerable&lt;T&gt; collection) {
    ///   foreach (var item in collection) {
    ///     //$ $END$
    ///   }
    /// }
    /// </code>
    ///     Applying the attribute on a template method parameter:
    ///     <code>
    /// [SourceTemplate]
    /// public static void something(this Entity x, [Macro(Expression = "guid()", Editable = -1)] string newguid) {
    ///   /*$ var $x$Id = "$newguid$" + x.ToString();
    ///   x.DoSomething($x$Id); */
    /// }
    /// </code>
    /// </example>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true )]
    internal sealed class MacroAttribute : Attribute
    {
        #region Properties

        /// <summary>
        ///     Allows specifying a macro that will be executed for a <see cref="SourceTemplateAttribute">source template</see>
        ///     parameter when the template is expanded.
        /// </summary>
        public String Expression { get; set; }

        /// <summary>
        ///     Allows specifying which occurrence of the target parameter becomes editable when the template is deployed.
        /// </summary>
        /// <remarks>
        ///     If the target parameter is used several times in the template, only one occurrence becomes editable;
        ///     other occurrences are changed synchronously. To specify the zero-based index of the editable occurrence,
        ///     use values >= 0. To make the parameter non-editable when the template is expanded, use -1.
        /// </remarks>
        /// >
        public Int32 Editable { get; set; }

        /// <summary>
        ///     Identifies the target parameter of a <see cref="SourceTemplateAttribute">source template</see> if the
        ///     <see cref="MacroAttribute" /> is applied on a template method.
        /// </summary>
        public String Target { get; set; }

        #endregion
    }
}