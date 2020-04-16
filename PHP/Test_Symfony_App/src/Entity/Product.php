<?php

namespace App\Entity;

use Doctrine\ORM\Mapping as ORM;

/**
 * @ORM\Entity(repositoryClass="App\Repository\ProductRepository")
 */
class Product
{
    /**
     * @ORM\Id()
     * @ORM\GeneratedValue()
     * @ORM\Column(type="integer")
     */
    private $id;
    
    public function getId()
    {
        return $this->id;
    }
    
    public function setId(int $id): self
    {
        $this->id = $id;
        
        return $this;
    }
    
    /**
     * @ORM\Column(type="string", length=255)
     */
    private $name;
    
    public function getName(): ?string
    {
        return $this->name;
    }
    
    public function setName(string $name): self
    {
        $this->name = $name;
        
        return $this;
    }
    
    /**
     * @ORM\Column(type="integer")
     */
    private $price;
    
    public function getPrice(): ?int
    {
        return $this->price;
    }
    
    public function setPrice(int $price): self
    {
        $this->price = $price;
        
        return $this;
    }

    /**
     * @ORM\Column(type="text")
     */
    private $description;

    public function getDescription(): ?string
    {
        return $this->description;
    }

    public function setDescription(string $description): self
    {
        $this->description = $description;

        return $this;
    }
    
    // ... getter and setter methods
}
