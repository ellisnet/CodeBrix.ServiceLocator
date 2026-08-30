namespace CodeBrix.ServiceLocation; //was previously: CommonServiceLocator; (was CodeBrix.ServiceLocator through v1.0.242.982)

/// <summary>
/// This delegate type is used to provide a method that will
/// return the current container. Used with the <see cref="ServiceLocator"/>
/// static accessor class.
/// </summary>
/// <returns>An <see cref="IServiceLocator"/>.</returns>
public delegate IServiceLocator ServiceLocatorProvider();
